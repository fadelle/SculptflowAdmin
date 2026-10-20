/* Search benchmark page (Pages/Knowledge/Benchmark) for one client. Drives /api/admin/knowledge-benchmark/{clientId}
   (data-api on #bm-root), which calls the main app's platform-admin benchmark API. While a run or a generation is in
   progress the page re-reads the dashboard every 2 seconds and shows the progress bar; it stops when nothing is running.
   Shared helpers and the detail popup: knowledge-benchmark-kit.js. */
(function () {
    'use strict';

    var root = document.getElementById('bm-root');
    if (!root || !window.BenchmarkKit) return;

    var K = window.BenchmarkKit, $ = K.$, el = K.el, pct = K.pct, num = K.num, fmtDate = K.fmtDate, cell = K.cell;
    var kit = K.create(root, 'bm-message');
    var api = kit.api, showMessage = kit.showMessage;
    var PAGE_SIZE = 25;
    var GENERATION_URL = root.dataset.generationUrl;

    var state = {
        caseView: 'manual',    // this page shows MANUAL cases only; generated cases live on their generation's page
        caseSkip: 0,
        caseTotal: 0,
        runId: null,           // the run whose results are shown
        resultFilter: '',
        wasRunning: false,
        genWasRunning: false,
        pendingGenId: null,    // the generation "Stop generating" cancels
        lastFailedShown: null,
        pollTimer: null
    };

    // ---------------------------------------------------------------- dashboard

    function loadDashboard() {
        return api('GET', '').then(function (d) {
            renderDashboard(d);
            return d;
        });
    }

    function renderDashboard(d) {
        var counts = $('bm-counts');
        counts.textContent = '';
        counts.appendChild(el('span', null, [
            el('strong', { text: String(d.totalCases) }), ' cases · ',
            el('strong', { text: String(d.reviewedCases) }), ' reviewed · ',
            el('strong', { text: String(d.staleCases) }), ' stale',
            el('span', { class: 'au-faint', text: '  (' + d.generatedCases + ' generated, ' + d.manualCases + ' manual)' })
        ]));

        var running = d.runInProgress;
        var generating = d.generationInProgress;
        $('bm-run').disabled = running || d.totalCases - d.staleCases <= 0;
        $('bm-generate').disabled = running || generating || !d.generatorConfigured;
        var note = $('bm-generate-note');
        if (!d.generatorConfigured) {
            note.hidden = false;
            note.textContent = 'Generate test cases needs the main app\'s question-generator webhook (N8n__KnowledgeBenchmarkWebhookUrl). Cases can still be added by hand.';
        } else if (generating) {
            note.hidden = false;
            note.textContent = 'Waiting for the question generator (n8n) to send the questions. This page updates by itself, so you can leave it open or come back later. "Stop generating" stops waiting; n8n may still finish, but its reply will be ignored.';
        } else if (note.dataset.busy !== '1') {
            note.hidden = true;
        }
        state.pendingGenId = d.pendingGenerationId || null;
        $('bm-stop').hidden = !generating;
        $('bm-stop').disabled = false;

        renderProgress(d);
        renderMetrics(d);

        // While a run is pending/running keep polling; when it finishes, refresh everything and open its results.
        if (running) {
            state.wasRunning = true;
            schedulePoll();
        } else if (state.wasRunning) {
            state.wasRunning = false;
            if (d.latestRun) state.runId = d.latestRun.id;
            refreshAll();
        }

        if (generating) {
            state.genWasRunning = true;
            schedulePoll();
        } else if (state.genWasRunning) {
            state.genWasRunning = false;
            onGenerationFinished();
        }
    }

    function schedulePoll() {
        if (state.pollTimer) return;
        state.pollTimer = setTimeout(function () {
            state.pollTimer = null;
            loadDashboard().catch(function () { /* transient: the next tick retries */ });
            loadRuns().catch(function () { });
            loadGenerations();
        }, 2000);
    }

    function renderProgress(d) {
        var run = d.latestRun;
        var box = $('bm-progress');
        if (run && (run.status === 'pending' || run.status === 'running')) {
            box.hidden = false;
            var total = Math.max(run.totalCases, 1);
            $('bm-progress-bar').style.setProperty('--value', Math.min(100, Math.round(run.processedCases * 100 / total)) + '%');
            $('bm-progress-text').textContent = run.status === 'pending'
                ? 'Waiting to start…'
                : 'Running the benchmark… ' + run.processedCases + ' of ' + run.totalCases + ' cases';
        } else {
            box.hidden = true;
            if (run && run.status === 'failed' && run.errorSummary && state.lastFailedShown !== run.id) {
                state.lastFailedShown = run.id;
                showMessage('The last benchmark run failed: ' + run.errorSummary, 'error');
            }
        }
    }

    function scopeLabel(caseScope, generationId) {
        if (caseScope === 'generation') return 'One generation (' + K.shortId(generationId) + ')';
        return { all: 'All cases', generated: 'Generated cases', reviewed: 'Reviewed cases' }[caseScope] || caseScope;
    }

    function renderMetrics(d) {
        var host = $('bm-metrics');
        host.textContent = '';
        var run = d.latestCompletedRun;
        if (!run) {
            host.appendChild(el('section', { class: 'au-panel au-panel--round au-panel--pad' }, [
                el('div', { class: 'au-muted', text: 'No benchmark has been run for this client yet. Generate or add some test cases, then click Run benchmark.' })
            ]));
            return;
        }

        var panel = el('section', { class: 'au-panel au-panel--round au-panel--pad au-stack', style: '--au-gap:12px' });
        panel.appendChild(el('div', { class: 'au-caption', text:
            'Latest completed run: ' + scopeLabel(run.caseScope, run.generationId) + ' · ' + fmtDate(run.completedAt || run.createdAt) + ' · ' +
            run.scoredCases + ' scored' + (run.staleCases ? ', ' + run.staleCases + ' stale (not scored)' : '') +
            (run.errorCases ? ', ' + run.errorCases + ' errored (not scored)' : '') }));

        panel.appendChild(el('span', { class: 'au-eyebrow', text: 'Strict chunk retrieval: the exact expected chunk was returned' }));
        panel.appendChild(K.statGrid([
            K.statTile('Top-1', pct(run.chunkTop1Accuracy), 'exact chunk ranked first'),
            K.statTile('Top-3', pct(run.chunkTop3Accuracy), 'exact chunk in the first 3'),
            K.statTile('Top-5', pct(run.chunkTop5Accuracy), 'exact chunk in the first 5'),
            K.statTile('MRR', num(run.chunkMrr), 'mean reciprocal rank (1 = always first)')
        ]));

        panel.appendChild(el('span', { class: 'au-eyebrow', text: 'Document retrieval (secondary): right topic, maybe the wrong chunk' }));
        panel.appendChild(K.statGrid([
            K.statTile('Top-1', pct(run.documentTop1Accuracy)),
            K.statTile('Top-3', pct(run.documentTop3Accuracy)),
            K.statTile('Top-5', pct(run.documentTop5Accuracy)),
            K.statTile('MRR', num(run.documentMrr)),
            K.statTile('Average latency', run.averageLatencyMs == null ? '—' : Math.round(run.averageLatencyMs) + ' ms', 'question → ranked chunks')
        ]));

        var s = run.settings || {};
        var notes = ['Settings used: ' + K.settingsLine(s) + '.'];
        if (s.indexedChunkCount != null) {
            notes.push('The index held ' + s.indexedChunkCount + ' active chunks averaging ' + (s.avgChunkChars == null ? '?' : Math.round(s.avgChunkChars)) + ' characters.');
        }
        if (s.topK != null && s.topK < 5) notes.push('Top K is ' + s.topK + ', so Top-5 can never be higher than Top-' + s.topK + '.');
        panel.appendChild(el('div', { class: 'au-caption', text: notes.join(' ') }));
        panel.appendChild(el('div', { class: 'au-caption', text:
            'Chunk metrics are strict (only the exact chunk counts). Document metrics are a diagnostic: a document hit without a chunk hit means the topic was found but not the specific passage.' }));
        host.appendChild(panel);
    }

    // ---------------------------------------------------------------- manual cases

    function loadCases() {
        var qs = '?view=' + encodeURIComponent(state.caseView) + '&skip=' + state.caseSkip + '&take=' + PAGE_SIZE;
        return api('GET', '/cases' + qs).then(function (res) {
            state.caseTotal = res.totalCount;
            renderCases(res.items);
        }).catch(function (e) { showMessage(e.message, 'error'); });
    }

    var caseHandlers = {
        open: function (c) { kit.openCase(c.id, function () { return Promise.all([loadCases(), loadDashboard()]); }); },
        toggleReviewed: function (c) {
            api('POST', '/cases/' + c.id + '/review', { reviewed: !c.isReviewed })
                .then(function () { return Promise.all([loadCases(), loadDashboard()]); })
                .catch(function (e) { showMessage(e.message, 'error'); });
        },
        remove: function (c) {
            K.ask('Delete test case', 'Delete this test case? Past run results keep their record of it.', 'Delete').then(function (ok) {
                if (!ok) return;
                api('DELETE', '/cases/' + c.id)
                    .then(function () { return Promise.all([loadCases(), loadDashboard()]); })
                    .catch(function (e) { showMessage(e.message, 'error'); });
            });
        }
    };

    function renderCases(items) {
        var body = $('bm-cases-body');
        body.textContent = '';
        if (!items.length) body.appendChild(K.emptyRow(5, 'No manual test cases yet. Click "Add manual case" to write one.'));
        items.forEach(function (c) { body.appendChild(kit.caseRow(c, caseHandlers)); });
        kit.renderPager('bm', state.caseSkip, state.caseTotal, PAGE_SIZE);
    }

    // ---------------------------------------------------------------- generate / run

    function generate() {
        var note = $('bm-generate-note');
        $('bm-generate').disabled = true;
        note.dataset.busy = '1';
        note.hidden = false;
        note.textContent = 'Sampling chunks and handing them to the question generator…';
        showMessage('');

        api('POST', '/cases/generate').then(function (res) {
            note.dataset.busy = '0';
            note.hidden = true;
            if (res.status === 'pending') {
                // Normal case: n8n acknowledged and sends the questions back on its own. The page polls until they arrive.
                state.genWasRunning = true;
                showMessage('Generation started: the question generator is writing questions for ' + res.chunksSent +
                    ' chunks. This page updates by itself when they arrive. Generation ' + res.generationId + '.');
            } else if (res.message) {
                showMessage(res.message);
            } else {
                // A workflow that answered synchronously: the questions are already stored.
                var details = (res.rejected || []).map(function (r) { return (r.question ? '"' + r.question + '": ' : '') + r.reason; });
                showMessage('Generation ' + res.generationId + ' completed: ' + res.casesCreated + ' test case' + (res.casesCreated === 1 ? '' : 's') +
                    ' created from ' + res.chunksSent + ' chunks' + (details.length ? ' (' + details.length + ' rejected)' : '') + '.', 'success', details);
            }
            return refreshAll();
        }).catch(function (e) {
            note.dataset.busy = '0';
            note.hidden = true;
            showMessage(e.message, 'error');
            refreshAll();
        });
    }

    /** "Stop generating": cancels the pending generation so Generate is free again (the main app ignores n8n's late reply). */
    function stopGenerating() {
        var id = state.pendingGenId;
        if (!id) return;
        K.ask('Stop waiting for the question generator?',
            'n8n may still finish writing questions, but this generation is cancelled and its reply will be ignored. A new generation can start right away.',
            'Stop generating').then(function (ok) {
            if (!ok) return;
            $('bm-stop').disabled = true;
            api('POST', '/generations/' + id + '/cancel').then(function () {
                state.genWasRunning = false;
                showMessage('Generation ' + id + ' was stopped. You can generate again.');
                return refreshAll();
            }).catch(function (e) {
                showMessage(e.message, 'error');
                return refreshAll();   // e.g. it had just completed: show the real state
            });
        });
    }

    /** Called once when a pending generation stops being pending: say how it ended and refresh the lists. */
    function onGenerationFinished() {
        loadGenerations().then(function (gens) {
            var g = gens && gens[0];
            if (g && g.status === 'completed') {
                showMessage('Generation ' + g.id + ' completed: ' + g.casesCreated + ' test case' + (g.casesCreated === 1 ? '' : 's') +
                    ' created from ' + g.chunksSent + ' chunks' + (g.rejectedCount ? ' (' + g.rejectedCount + ' rejected; open it below to see why)' : '') + '.', 'success');
            } else if (g && g.status === 'failed') {
                showMessage('Generation ' + g.id + ' failed: ' + (g.errorMessage || 'unknown error'), 'error');
            } else if (g && g.status === 'cancelled') {
                showMessage('Generation ' + g.id + ' was stopped.');
            }
            loadCases();
            loadDashboard().catch(function () { });
        });
    }

    function startRun(body, startedMessage) {
        $('bm-run').disabled = true;
        showMessage('');
        api('POST', '/runs', body).then(function () {
            state.wasRunning = true;
            if (startedMessage) showMessage(startedMessage);
            return loadDashboard();
        }).catch(function (e) {
            showMessage(e.message, 'error');
            loadDashboard().catch(function () { });
        });
    }

    // ---------------------------------------------------------------- generations

    function loadGenerations() {
        return api('GET', '/generations?take=20').then(function (gens) {
            renderGenerations(gens);
            return gens;
        }).catch(function (e) { showMessage(e.message, 'error'); return []; });
    }

    function renderGenerations(gens) {
        var body = $('bm-gens-body');
        body.textContent = '';
        if (!gens.length) {
            body.appendChild(K.emptyRow(12, 'No generations yet. Click "Generate test cases" to ask the question generator for some.'));
            return;
        }
        gens.forEach(function (g) {
            var s = g.latestScores;
            var pending = g.status === 'pending';
            body.appendChild(el('tr', null, [
                el('td', null, [el('div', { text: fmtDate(g.requestedAt) }), el('div', { class: 'au-mono au-faint', title: g.id, text: K.shortId(g.id) })]),
                el('td', null, [K.generationChip(g)]),
                cell(String(g.chunksSent), true),
                cell(pending ? '—' : String(g.questionsReturned), true),
                cell(pending ? '—' : g.casesCreated + (g.casesRemaining !== g.casesCreated ? ' (' + g.casesRemaining + ' left)' : ''), true),
                cell(pending ? '—' : String(g.rejectedCount), true),
                cell(s ? pct(s.chunkTop1) : '—', true),
                cell(s ? pct(s.chunkTop3) : '—', true),
                cell(s ? pct(s.chunkTop5) : '—', true),
                cell(s ? num(s.chunkMrr) : '—', true),
                el('td', { class: 'au-caption', text: s ? s.scoredCases + ' cases · ' + fmtDate(s.runAt) : 'not run yet' }),
                el('td', null, [el('div', { class: 'au-row' }, [
                    el('a', { class: 'au-btn au-btn--outlined au-btn--sm', href: GENERATION_URL.replace('{id}', g.id), text: 'Open' }),
                    el('button', {
                        type: 'button', class: 'au-btn au-btn--primary au-btn--sm', text: 'Run',
                        title: 'Run the benchmark for just this generation’s ' + g.casesRemaining + ' case(s)',
                        disabled: (g.casesRemaining === 0 || $('bm-run').disabled) ? true : null,
                        onclick: function () { startRun({ scope: 'generation', generationId: g.id }, 'Running the benchmark for generation ' + g.id + '…'); }
                    })
                ])])
            ]));
        });
    }

    // ---------------------------------------------------------------- runs + results

    function loadRuns() {
        return api('GET', '/runs?take=20').then(renderRuns);
    }

    function renderRuns(runs) {
        var body = $('bm-runs-body');
        body.textContent = '';
        if (!runs.length) {
            body.appendChild(K.emptyRow(11, 'No runs yet. Add or generate test cases, then click Run benchmark.'));
            return;
        }
        var short = { all: 'All', generated: 'Generated', reviewed: 'Reviewed' };
        runs.forEach(function (r) {
            var tone = { completed: 'au-chip--success', running: 'au-chip--warning', pending: 'au-chip--outlined', failed: 'au-chip--error' }[r.status] || 'au-chip--outlined';
            var scope = r.caseScope === 'generation' ? 'Generation ' + K.shortId(r.generationId) : (short[r.caseScope] || r.caseScope);
            body.appendChild(el('tr', null, [
                el('td', { text: fmtDate(r.startedAt || r.createdAt) }),
                el('td', { text: scope + ' · ' + r.scoredCases + ' scored' + (r.staleCases ? ' · ' + r.staleCases + ' stale' : '') }),
                el('td', null, [K.chip(tone, r.status === 'running' ? 'Running ' + r.processedCases + '/' + r.totalCases : r.status.charAt(0).toUpperCase() + r.status.slice(1), r.errorSummary)]),
                cell(pct(r.chunkTop1Accuracy), true),
                cell(pct(r.chunkTop3Accuracy), true),
                cell(pct(r.chunkTop5Accuracy), true),
                cell(num(r.chunkMrr), true),
                cell(pct(r.documentTop3Accuracy), true),
                cell(r.averageLatencyMs == null ? '—' : Math.round(r.averageLatencyMs) + ' ms', true),
                el('td', { class: 'bm-clip' }, [el('span', { class: 'au-ellipsis au-caption', style: 'display:block', title: K.settingsLine(r.settings), text: K.settingsLine(r.settings) })]),
                el('td', null, [el('button', { type: 'button', class: 'au-btn au-btn--outlined au-btn--sm', text: 'View results', onclick: function () { selectRun(r.id); } })])
            ]));
        });
    }

    function selectRun(runId) {
        state.runId = runId;
        loadResults().then(function () { $('bm-results-section').scrollIntoView({ behavior: 'smooth', block: 'start' }); });
    }

    function loadResults() {
        var section = $('bm-results-section');
        if (!state.runId) { section.hidden = true; return Promise.resolve(); }
        var qs = '?skip=0&take=500' + (state.resultFilter ? '&classification=' + encodeURIComponent(state.resultFilter) : '');
        api('GET', '/runs/' + state.runId + '/generations').then(renderRunGenerations).catch(function () { /* optional panel */ });
        return api('GET', '/runs/' + state.runId + '/results' + qs).then(function (res) {
            section.hidden = false;
            $('bm-results-title').textContent = 'Run results (' + res.totalCount + ')';
            renderResults(res.items);
        }).catch(function (e) { showMessage(e.message, 'error'); });
    }

    /** "Scores by generation" for the selected run (manual cases, with no generation, are grouped last). */
    function renderRunGenerations(rows) {
        var body = $('bm-run-gens-body');
        body.textContent = '';
        $('bm-run-gens').hidden = rows.length === 0;
        rows.forEach(function (b) {
            var s = b.scores;
            body.appendChild(el('tr', null, [
                el('td', null, b.generationId
                    ? [el('div', { text: fmtDate(b.generationRequestedAt) }), el('div', { class: 'au-mono au-faint', title: b.generationId, text: K.shortId(b.generationId) })]
                    : [el('span', { class: 'au-muted', text: 'No generation (manual cases)' })]),
                cell(String(s.scoredCases), true),
                cell(pct(s.chunkTop1), true), cell(pct(s.chunkTop3), true), cell(pct(s.chunkTop5), true),
                cell(num(s.chunkMrr), true), cell(pct(s.documentTop3), true)
            ]));
        });
    }

    function renderResults(items) {
        var body = $('bm-results-body');
        body.textContent = '';
        if (!items.length) {
            body.appendChild(K.emptyRow(7, 'No results match this filter.'));
            return;
        }
        items.forEach(function (r) {
            var open = function (e) { if (e) e.preventDefault(); kit.openResult(r.runId, r.id); };
            body.appendChild(el('tr', { tabindex: '0', onclick: open, onkeydown: function (e) { if (e.key === 'Enter') open(e); } }, [
                el('td', { class: 'bm-question' }, [el('span', { class: 'au-ellipsis', style: 'display:block', title: r.question, text: r.question })]),
                el('td', null, [K.badgeFor(r.classification, r.expectedChunkRank)]),
                cell(r.expectedChunkRank == null ? '—' : '#' + r.expectedChunkRank, true),
                cell(r.expectedDocumentBestRank == null ? '—' : '#' + r.expectedDocumentBestRank, true),
                cell(r.expectedChunkScore == null ? '—' : num(r.expectedChunkScore, 3) + (r.expectedBelowThreshold ? ' (below min)' : ''), true),
                cell(r.latencyMs == null ? '—' : r.latencyMs + ' ms', true),
                el('td', null, [el('button', { type: 'button', class: 'au-btn au-btn--outlined au-btn--sm', text: 'Analyze', onclick: function (e) { e.stopPropagation(); open(); } })])
            ]));
        });
    }

    // ---------------------------------------------------------------- add manual case

    function toggleAddForm(show) {
        var form = $('bm-add-form');
        form.hidden = show === undefined ? !form.hidden : !show;
        if (!form.hidden) loadSourceDocuments();
    }

    function loadSourceDocuments() {
        var sel = $('bm-add-doc');
        api('GET', '/source-documents').then(function (docs) {
            sel.textContent = '';
            sel.appendChild(el('option', { value: '', text: docs.length ? 'Choose a document…' : 'No active documents with chunks' }));
            docs.forEach(function (d) { sel.appendChild(el('option', { value: d.id, text: d.title + ' (' + d.chunkCount + ' chunk' + (d.chunkCount === 1 ? '' : 's') + ')' })); });
        }).catch(function (e) { showMessage(e.message, 'error'); });
    }

    function loadSourceChunks(documentId) {
        var sel = $('bm-add-chunk');
        sel.textContent = '';
        if (!documentId) {
            sel.disabled = true;
            sel.appendChild(el('option', { value: '', text: 'Choose a document first' }));
            return;
        }
        api('GET', '/source-documents/' + documentId + '/chunks').then(function (chunks) {
            sel.disabled = false;
            sel.appendChild(el('option', { value: '', text: 'Choose the chunk…' }));
            chunks.forEach(function (c) { sel.appendChild(el('option', { value: c.id, text: '#' + (c.chunkIndex + 1) + ' · ' + c.preview })); });
        }).catch(function (e) { showMessage(e.message, 'error'); });
    }

    function saveManualCase() {
        var question = $('bm-add-question').value;
        var documentId = $('bm-add-doc').value;
        var chunkId = $('bm-add-chunk').value;
        if (!question.trim() || !documentId || !chunkId) {
            showMessage('Enter the question and choose the document and chunk that answer it.', 'error');
            return;
        }
        var btn = $('bm-add-save');
        btn.disabled = true;
        api('POST', '/cases', { question: question, documentId: documentId, chunkId: chunkId }).then(function () {
            $('bm-add-question').value = '';
            $('bm-add-doc').value = '';
            loadSourceChunks('');
            toggleAddForm(false);
            showMessage('Case added.', 'success');
            return refreshAll();
        }).catch(function (e) { showMessage(e.message, 'error'); }).then(function () { btn.disabled = false; });
    }

    // ---------------------------------------------------------------- wiring

    function refreshAll() {
        return Promise.all([
            loadDashboard().catch(function (e) { showMessage(e.message, 'error'); }),
            loadCases(), loadRuns().catch(function () { }), loadGenerations(), state.runId ? loadResults() : Promise.resolve()
        ]);
    }

    $('bm-generate').addEventListener('click', generate);
    $('bm-stop').addEventListener('click', stopGenerating);
    $('bm-run').addEventListener('click', function () { startRun({ scope: $('bm-scope').value }); });
    $('bm-cases-prev').addEventListener('click', function () { state.caseSkip = Math.max(0, state.caseSkip - PAGE_SIZE); loadCases(); });
    $('bm-cases-next').addEventListener('click', function () { state.caseSkip += PAGE_SIZE; loadCases(); });
    $('bm-result-filter').addEventListener('change', function (e) { state.resultFilter = e.target.value; loadResults(); });
    $('bm-add-toggle').addEventListener('click', function () { toggleAddForm(); });
    $('bm-add-cancel').addEventListener('click', function () { toggleAddForm(false); });
    $('bm-add-doc').addEventListener('change', function (e) { loadSourceChunks(e.target.value); });
    $('bm-add-save').addEventListener('click', saveManualCase);

    loadDashboard().then(function (d) {
        // Show the newest completed run's results by default.
        if (d.latestCompletedRun) state.runId = d.latestCompletedRun.id;
        return Promise.all([loadCases(), loadRuns(), loadGenerations(), state.runId ? loadResults() : null]);
    }).catch(function (e) {
        showMessage(e.message, 'error');
        ['bm-cases-body', 'bm-gens-body', 'bm-runs-body'].forEach(function (id) {
            var body = $(id);
            body.textContent = '';
            body.appendChild(K.emptyRow(body.closest('table').querySelectorAll('thead th').length, 'Could not load. See the message above.'));
        });
        $('bm-counts').textContent = '—';
        $('bm-generate').disabled = true;
        $('bm-run').disabled = true;
    });
})();
