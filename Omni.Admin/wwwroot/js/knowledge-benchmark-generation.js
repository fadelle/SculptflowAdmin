/* One search-benchmark generation's page (Pages/Knowledge/BenchmarkGeneration, /Knowledge/Benchmark/Generations/{id}).
   Drives the same /api/admin/knowledge-benchmark/{clinicId} API as the Search benchmark page (data-api and data-id on
   #bmg-root), always for this one generation. Shared helpers and the detail popup: knowledge-benchmark-kit.js. */
(function () {
    'use strict';

    var root = document.getElementById('bmg-root');
    if (!root || !window.BenchmarkKit) return;

    var K = window.BenchmarkKit, $ = K.$, el = K.el, pct = K.pct, num = K.num, fmtDate = K.fmtDate;
    var kit = K.create(root, 'bmg-message');
    var api = kit.api, showMessage = kit.showMessage;
    var PAGE_SIZE = 25;
    var GEN_ID = root.dataset.id;

    var state = { caseView: 'all', caseSkip: 0, caseTotal: 0 };

    // ---------------------------------------------------------------- summary (the generation itself)

    function loadSummary() {
        return api('GET', '/generations/' + GEN_ID).then(function (d) {
            renderSummary(d);
            return d;
        }).catch(function (e) {
            $('bmg-summary').textContent = '—';
            showMessage(e.message, 'error');
        });
    }

    function renderSummary(d) {
        var g = d.summary;
        document.title = 'Generation ' + K.shortId(g.id) + ' · Search benchmark';

        var box = $('bmg-summary');
        box.textContent = '';
        var stack = el('div', { class: 'au-stack', style: '--au-gap:10px' });
        stack.appendChild(el('div', { class: 'au-row au-row--wrap' }, [
            K.generationChip(g),
            el('span', { text: 'Requested ' + fmtDate(g.requestedAt) + (g.completedAt ? ' · finished ' + fmtDate(g.completedAt) : '') }),
            el('span', { class: 'au-mono au-faint', title: 'Generation id', text: g.id })
        ]));
        stack.appendChild(el('div', { class: 'au-muted', text:
            g.chunksSent + ' chunks sent · ' + g.questionsReturned + ' questions returned · ' + g.casesCreated + ' cases created · ' +
            g.rejectedCount + ' rejected' + (g.casesRemaining !== g.casesCreated ? ' · ' + g.casesRemaining + ' cases left (some deleted)' : '') }));

        if (g.errorMessage) stack.appendChild(el('div', { class: 'au-alert au-alert--error' }, [el('div', { class: 'au-alert__body', text: g.errorMessage })]));
        if (g.status === 'pending') {
            stack.appendChild(el('div', { class: 'au-alert' }, [el('div', { class: 'au-alert__body', text: 'Still waiting for the question generator to call back. Reload this page to check again.' })]));
        }
        if (g.casesRemaining > 0) {
            stack.appendChild(el('div', null, [
                el('button', { type: 'button', class: 'au-btn au-btn--primary au-btn--sm', id: 'bmg-run', text: 'Run benchmark for this generation', onclick: runThisGeneration })
            ]));
        }
        box.appendChild(stack);

        renderScores(g.latestScores);

        var rejBox = $('bmg-rejected');
        if (d.rejected.length) {
            rejBox.hidden = false;
            $('bmg-rejected-title').textContent = 'Rejected questions (' + g.rejectedCount + (g.rejectedCount > d.rejected.length ? ', first ' + d.rejected.length + ' shown' : '') + ')';
            var tb = $('bmg-rejected-body');
            tb.textContent = '';
            d.rejected.forEach(function (r) {
                tb.appendChild(el('tr', null, [el('td', { class: 'au-wrap', text: r.question || '—' }), el('td', { class: 'au-wrap', text: r.reason })]));
            });
        } else {
            rejBox.hidden = true;
        }

        $('bmg-chunks-count').textContent = String(d.sentChunks.length);
        var list = $('bmg-chunks-list');
        list.textContent = '';
        d.sentChunks.forEach(function (c) {
            list.appendChild(el('li', null, [c.documentTitle + ' ', el('span', { class: 'au-faint', text: '· chunk ' + K.shortId(c.chunkId) })]));
        });

        $('bmg-raw-wrap').hidden = !d.rawResponse;
        $('bmg-raw').textContent = d.rawResponse || '';
    }

    function renderScores(s) {
        var host = $('bmg-scores');
        host.textContent = '';
        var panel = el('section', { class: 'au-panel au-panel--round au-panel--pad au-stack', style: '--au-gap:12px' });
        if (!s) {
            panel.appendChild(el('div', { class: 'au-muted', text: 'Not run yet. Click "Run benchmark for this generation" above.' }));
        } else {
            panel.appendChild(el('span', { class: 'au-eyebrow', text: 'Scores (latest completed run, ' + s.scoredCases + ' scored ' + fmtDate(s.runAt) + ')' }));
            panel.appendChild(K.statGrid([
                K.statTile('Chunk top-1', pct(s.chunkTop1)), K.statTile('Chunk top-3', pct(s.chunkTop3)),
                K.statTile('Chunk top-5', pct(s.chunkTop5)), K.statTile('Chunk MRR', num(s.chunkMrr)),
                K.statTile('Doc top-3', pct(s.documentTop3))
            ]));
        }
        host.appendChild(panel);
    }

    function runThisGeneration() {
        var btn = $('bmg-run');
        if (btn) btn.disabled = true;
        showMessage('');
        api('POST', '/runs', { scope: 'generation', generationId: GEN_ID }).then(function () {
            showMessage('Running the benchmark for this generation…');
            setTimeout(waitForRun, 1500);
        }).catch(function (e) {
            showMessage(e.message, 'error');
            if (btn) btn.disabled = false;
        });
    }

    /** Re-reads the clinic's dashboard only to know when the run this page started has finished, then reloads the
        summary and cases to show the fresh scores. */
    function waitForRun() {
        api('GET', '').then(function (dash) {
            if (dash.runInProgress) {
                setTimeout(waitForRun, 2000);
            } else {
                showMessage(dash.latestRun && dash.latestRun.status === 'failed'
                    ? 'The run failed: ' + (dash.latestRun.errorSummary || 'unknown error') : 'The run finished.', dash.latestRun && dash.latestRun.status === 'failed' ? 'error' : 'success');
                loadSummary();
                loadCases();
            }
        }).catch(function () { /* transient: reload to check */ });
    }

    // ---------------------------------------------------------------- this generation's cases

    function loadCases() {
        var qs = '?view=' + encodeURIComponent(state.caseView) + '&generationId=' + encodeURIComponent(GEN_ID) + '&skip=' + state.caseSkip + '&take=' + PAGE_SIZE;
        return api('GET', '/cases' + qs).then(function (res) {
            state.caseTotal = res.totalCount;
            renderCases(res.items);
        }).catch(function (e) {
            showMessage(e.message, 'error');
            var body = $('bmg-cases-body');
            body.textContent = '';
            body.appendChild(K.emptyRow(5, 'Could not load the cases. See the message above.'));
        });
    }

    var caseHandlers = {
        open: function (c) { kit.openCase(c.id, loadCases); },
        toggleReviewed: function (c) {
            api('POST', '/cases/' + c.id + '/review', { reviewed: !c.isReviewed })
                .then(loadCases)
                .catch(function (e) { showMessage(e.message, 'error'); });
        },
        remove: function (c) {
            K.ask('Delete test case', 'Delete this test case? Past run results keep their record of it.', 'Delete').then(function (ok) {
                if (!ok) return;
                api('DELETE', '/cases/' + c.id)
                    .then(function () { return Promise.all([loadCases(), loadSummary()]); })
                    .catch(function (e) { showMessage(e.message, 'error'); });
            });
        }
    };

    function renderCases(items) {
        var body = $('bmg-cases-body');
        body.textContent = '';
        if (!items.length) body.appendChild(K.emptyRow(5, state.caseView === 'all' ? 'This generation has no test cases left.' : 'No cases match this filter.'));
        items.forEach(function (c) { body.appendChild(kit.caseRow(c, caseHandlers)); });
        kit.renderPager('bmg', state.caseSkip, state.caseTotal, PAGE_SIZE);
    }

    // ---------------------------------------------------------------- wiring

    $('bmg-case-view').addEventListener('change', function (e) { state.caseView = e.target.value; state.caseSkip = 0; loadCases(); });
    $('bmg-cases-prev').addEventListener('click', function () { state.caseSkip = Math.max(0, state.caseSkip - PAGE_SIZE); loadCases(); });
    $('bmg-cases-next').addEventListener('click', function () { state.caseSkip += PAGE_SIZE; loadCases(); });

    loadSummary();
    loadCases();
})();
