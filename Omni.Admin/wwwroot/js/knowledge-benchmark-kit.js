/* Search benchmark — what both benchmark pages share (knowledge-benchmark.js, knowledge-benchmark-generation.js): the API
   call, formatting, chips, the test-case table row, and the detail popup (#bm-dialog, _BenchmarkDialog) with its case
   editor and failed-case analysis. The API root comes from the page's data-api (/api/admin/knowledge-benchmark/{clinicId}).
   Every value from the server is written with textContent (never innerHTML), so question/chunk text can't inject markup. */
(function () {
    'use strict';

    function $(id) { return document.getElementById(id); }

    function el(tag, attrs, kids) {
        var node = document.createElement(tag);
        if (attrs) {
            Object.keys(attrs).forEach(function (k) {
                var v = attrs[k];
                if (v === null || v === undefined || v === false) return;
                if (k === 'class') node.className = v;
                else if (k === 'text') node.textContent = v;
                else if (k.indexOf('on') === 0) node.addEventListener(k.slice(2), v);
                else node.setAttribute(k, v === true ? '' : v);
            });
        }
        (kids || []).forEach(function (kid) {
            if (kid === null || kid === undefined) return;
            node.appendChild(typeof kid === 'string' ? document.createTextNode(kid) : kid);
        });
        return node;
    }

    function pct(v) {
        if (v === null || v === undefined) return '—';
        return (v * 100).toFixed(1).replace(/\.0$/, '') + '%';
    }

    function num(v, digits) {
        return v === null || v === undefined ? '—' : Number(v).toFixed(digits === undefined ? 2 : digits);
    }

    function fmtDate(iso) {
        if (!iso) return '—';
        return new Date(iso).toLocaleString(undefined, { month: 'short', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' });
    }

    function shortId(id) { return (id || '').slice(0, 8) + '…'; }

    function chip(tone, text, title) {
        return el('span', { class: 'au-chip ' + tone, text: text, title: title || null });
    }

    // Result of one case in a run (docs/UI_GUIDE.md §9.3 tones).
    function badgeFor(classification, rank) {
        switch (classification) {
            case 'EXACT_CHUNK_HIT': return chip('au-chip--success', 'Exact chunk' + (rank ? ' · #' + rank : ''));
            case 'DOCUMENT_ONLY_HIT': return chip('au-chip--warning', 'Document only' + (rank ? ' · #' + rank : ''));
            case 'MISS': return chip('au-chip--error', 'Miss');
            case 'STALE_CASE': return chip('au-chip--outlined', 'Stale');
            case 'ERROR': return chip('au-chip--outlined au-chip--error', 'Error');
            default: return el('span', { class: 'au-faint', text: '—' });
        }
    }

    // Generation status: pending → waiting, cancelled → switched off.
    function generationChip(g) {
        var tone = { completed: 'au-chip--success', pending: 'au-chip--warning', failed: 'au-chip--error' }[g.status] || 'au-chip--outlined';
        var text = g.status === 'pending' ? 'Waiting for n8n…' : g.status === 'cancelled' ? 'Stopped' : g.status.charAt(0).toUpperCase() + g.status.slice(1);
        return chip(tone, text, g.errorMessage);
    }

    function reviewedChip(isReviewed) {
        return chip(isReviewed ? 'au-chip--success' : 'au-chip--outlined', isReviewed ? 'Reviewed' : 'Not reviewed');
    }

    function settingsLine(s) {
        if (!s) return '';
        var parts = [];
        if (s.chunkSizeTokens != null) parts.push('chunk ' + s.chunkSizeTokens + ' / overlap ' + s.chunkOverlapTokens + ' tokens');
        if (s.topK != null) parts.push('Top K ' + s.topK);
        if (s.minimumSimilarity != null) parts.push('min similarity ' + s.minimumSimilarity);
        if (s.embeddingModel) parts.push(s.embeddingModel + (s.vectorDimension ? ' (' + s.vectorDimension + 'd)' : ''));
        if (s.similarityMethod) parts.push(s.similarityMethod);
        return parts.join(' · ');
    }

    function statTile(label, value, hint) {
        return el('div', { class: 'au-stat' }, [
            el('span', { class: 'au-eyebrow', text: label }),
            el('div', { class: 'au-stat__value', text: value }),
            hint ? el('div', { class: 'au-stat__hint', text: hint }) : null
        ]);
    }

    function statGrid(tiles) { return el('div', { class: 'bm-stats' }, tiles); }

    function emptyRow(cols, text) {
        return el('tr', { class: 'bm-empty' }, [el('td', { colspan: String(cols), text: text })]);
    }

    function cell(text, numeric) { return el('td', { class: numeric ? 'au-right' : null, text: text }); }

    // Ask before a destructive action with the kit's confirm dialog (the browser's own if the kit is missing).
    function ask(title, message, confirmLabel) {
        if (window.Aurora && window.Aurora.confirm) {
            return window.Aurora.confirm({ title: title, message: message, confirmLabel: confirmLabel, tone: 'danger' });
        }
        return Promise.resolve(window.confirm(message));
    }

    /** One page's kit: its API root (data-api on rootEl) and its message box. */
    function create(rootEl, messageId) {
        var API = rootEl.dataset.api;

        function api(method, path, body) {
            var opts = { method: method, credentials: 'same-origin', headers: { 'Accept': 'application/json' } };
            if (body !== undefined) {
                opts.headers['Content-Type'] = 'application/json';
                opts.body = JSON.stringify(body);
            }
            return fetch(API + path, opts).then(function (res) {
                if (res.status === 204) return null;
                if (res.status === 401) throw new Error('Your session has ended. Reload the page and sign in again.');
                return res.text().then(function (text) {
                    var data = null;
                    try { data = text ? JSON.parse(text) : null; } catch (e) { /* not JSON */ }
                    if (!res.ok) throw new Error((data && (data.error || data.title)) || ('Request failed (' + res.status + ').'));
                    return data;
                });
            });
        }

        /** kind: 'error' | 'success' | anything else (info). */
        function showMessage(text, kind, detailsList) {
            var box = $(messageId);
            box.textContent = '';
            if (!text) return;
            var tone = kind === 'error' ? ' au-alert--error' : kind === 'success' ? ' au-alert--success' : '';
            var icon = kind === 'error' ? 'error' : kind === 'success' ? 'check_circle' : 'info';
            var body = el('div', { class: 'au-alert__body' }, [text]);
            if (detailsList && detailsList.length) {
                var ul = el('ul', { class: 'bm-details-list' });
                detailsList.forEach(function (d) { ul.appendChild(el('li', { text: d })); });
                body.appendChild(el('details', null, [el('summary', { text: 'Details' }), ul]));
            }
            box.appendChild(el('div', { class: 'au-alert' + tone, role: kind === 'error' ? 'alert' : 'status' }, [
                el('span', { class: 'au-icon au-alert__icon', 'aria-hidden': 'true', text: icon }), body
            ]));
        }

        function openDialog(title, bodyNode) {
            $('bm-dialog-heading').textContent = title;
            var body = $('bm-dialog-body');
            body.textContent = '';
            body.appendChild(bodyNode);
            var dlg = $('bm-dialog');
            if (!dlg.open) dlg.showModal();
        }

        /** The failed-case analysis: verdict, the settings in force, and the ranked chunks retrieval actually returned. */
        function analysisNode(detail) {
            var r = detail.result;
            var s = detail.runSettings || {};
            var wrap = el('div', { class: 'au-stack', style: '--au-gap:10px' });

            var verdict, cls;
            if (r.classification === 'EXACT_CHUNK_HIT') {
                cls = 'au-alert--success';
                verdict = 'Exact chunk found at rank ' + r.expectedChunkRank + (r.expectedChunkRank === 1 ? ', retrieved first.' : '.');
            } else if (r.classification === 'DOCUMENT_ONLY_HIT') {
                cls = 'au-alert--warning';
                verdict = 'Exact chunk not found, but the correct document was found at rank ' + r.expectedDocumentBestRank +
                    '. Retrieval understood the topic but did not return the specific passage. This can point to chunk size, overlap, ranking or how the content is structured.';
            } else if (r.classification === 'MISS') {
                cls = 'au-alert--error';
                verdict = 'Miss: neither the expected chunk nor its document was returned.';
                if (r.expectedBelowThreshold) {
                    verdict += ' The expected chunk did rank in the top ' + (s.topK || '') + ' with similarity ' + num(r.expectedChunkScore, 3) +
                        ', but that is below the minimum similarity (' + (s.minimumSimilarity != null ? s.minimumSimilarity : '?') + '), so search dropped it.';
                }
            } else if (r.classification === 'STALE_CASE') {
                cls = '';
                verdict = 'Stale: the expected chunk changed or no longer exists, so this case was not scored.';
            } else {
                cls = '';
                verdict = 'Retrieval errored, so this case was not scored: ' + (r.errorMessage || 'unknown error');
            }
            wrap.appendChild(el('div', { class: 'au-alert ' + cls }, [el('div', { class: 'au-alert__body', text: verdict })]));

            if (r.classification !== 'STALE_CASE') {
                wrap.appendChild(el('div', { class: 'au-caption', text: 'Run settings: ' + settingsLine(s) + (r.latencyMs != null ? ' · ' + r.latencyMs + ' ms' : '') }));
            }

            wrap.appendChild(el('div', null, [
                el('span', { class: 'au-eyebrow', text: 'Expected' }),
                el('div', { class: 'au-strong', text: r.expectedDocumentTitle || '—' }),
                el('div', { class: 'bm-block', text: detail.expectedChunkContent || r.expectedChunkPreview || '' })
            ]));

            var rows = detail.retrieved || [];
            var returned = rows.filter(function (x) { return x.passedThreshold; }).length;
            var dropped = rows.length - returned;
            wrap.appendChild(el('span', { class: 'au-eyebrow', text: 'Retrieved results (' + returned + ' returned' + (dropped ? ', ' + dropped + ' below the minimum similarity' : '') + ')' }));
            if (!rows.length) {
                wrap.appendChild(el('div', { class: 'au-muted', text: r.classification === 'STALE_CASE' || r.classification === 'ERROR' ? 'Nothing was retrieved.' : 'Search returned no results.' }));
                return wrap;
            }

            var tbody = el('tbody');
            rows.forEach(function (x) {
                var tags = [];
                if (x.isExpectedChunk) tags.push(chip('au-chip--success', 'Expected chunk'));
                else if (x.isExpectedDocument) tags.push(chip('au-chip--warning', 'Expected document'));
                if (!x.passedThreshold) tags.push(chip('au-chip--outlined', 'Below min similarity, dropped'));
                var rowClass = x.isExpectedChunk ? 'expected-chunk' : (x.isExpectedDocument ? 'expected-doc' : '');
                if (!x.passedThreshold) rowClass += ' dropped';
                tbody.appendChild(el('tr', { class: rowClass || null }, [
                    cell('#' + x.rank),
                    el('td', null, [el('div', { class: 'au-strong', text: x.title }), tags.length ? el('div', { class: 'au-row au-row--wrap', style: '--au-gap:4px;margin-top:4px' }, tags) : null]),
                    cell(num(x.score, 3), true),
                    el('td', { text: x.preview })
                ]));
            });
            wrap.appendChild(el('div', { class: 'au-table-card' }, [el('div', { class: 'au-table-scroll' }, [el('table', { class: 'au-table bm-retrieved', 'data-nosort': true }, [
                el('thead', null, [el('tr', null, [el('th', { text: 'Rank' }), el('th', { text: 'Document' }), el('th', { class: 'au-right', text: 'Score' }), el('th', { text: 'Chunk preview' })])]),
                tbody
            ])])]));
            return wrap;
        }

        /** The test-case popup: edit the question, mark reviewed, see the expected chunk and the latest result.
            onChanged runs after a save so the page can refresh its lists. */
        function openCase(caseId, onChanged) {
            api('GET', '/cases/' + caseId).then(function (d) {
                var c = d.case;
                var wrap = el('div', { class: 'au-stack', style: '--au-gap:12px' });

                if (c.isStale) {
                    wrap.appendChild(el('div', { class: 'au-alert au-alert--warning' }, [el('div', { class: 'au-alert__body' }, [
                        el('strong', { text: 'Stale, so excluded from scoring. ' }), c.staleExplanation || '',
                        ' Delete this case and generate a new one for the current text.'
                    ])]));
                }

                var questionBox = el('textarea', { class: 'au-textarea', rows: '2', maxlength: '500', id: 'bm-edit-question', 'aria-label': 'Patient question' });
                questionBox.value = c.question;
                var status = el('div', { class: 'au-caption', role: 'status', style: 'min-height:1.2em' });

                function save(markReviewed) {
                    status.textContent = 'Saving…';
                    api('PUT', '/cases/' + c.id, { question: questionBox.value })
                        .then(function () { return markReviewed ? api('POST', '/cases/' + c.id + '/review', { reviewed: true }) : null; })
                        .then(function () { status.textContent = 'Saved.'; return onChanged && onChanged(); })
                        .catch(function (e) { status.textContent = e.message; });
                }

                wrap.appendChild(el('label', { class: 'au-form-field', for: 'bm-edit-question' }, [el('span', { class: 'au-form-label', text: 'Patient question' })]));
                wrap.appendChild(questionBox);
                wrap.appendChild(el('div', { class: 'au-row au-row--wrap' }, [
                    el('button', { type: 'button', class: 'au-btn au-btn--primary au-btn--sm', text: 'Save', onclick: function () { save(false); } }),
                    el('button', { type: 'button', class: 'au-btn au-btn--outlined au-btn--sm', text: 'Save & mark reviewed', onclick: function () { save(true); } }),
                    reviewedChip(c.isReviewed),
                    chip(c.caseType === 'manual' ? 'au-chip--outlined au-chip--info' : 'au-chip--outlined', c.caseType === 'manual' ? 'Manual' : 'Generated')
                ]));
                wrap.appendChild(status);
                if (c.generationId) wrap.appendChild(el('div', { class: 'au-caption' }, ['Generation ', el('span', { class: 'au-mono', text: c.generationId })]));

                wrap.appendChild(el('div', null, [
                    el('span', { class: 'au-eyebrow', text: 'Expected document' }),
                    el('div', { class: 'au-strong', text: c.expectedDocumentTitle || '—' })
                ]));
                wrap.appendChild(el('div', null, [
                    el('span', { class: 'au-eyebrow', text: d.expectedChunkContent ? 'Expected chunk (source text)' : 'Expected chunk (saved excerpt; the chunk no longer exists)' }),
                    el('div', { class: 'bm-block', text: d.expectedChunkContent || c.expectedChunkPreview || '' })
                ]));

                if (d.latestResult) {
                    wrap.appendChild(el('span', { class: 'au-eyebrow', text: 'Latest result · ' + fmtDate(d.latestResult.result.createdAt) }));
                    wrap.appendChild(analysisNode(d.latestResult));
                } else {
                    wrap.appendChild(el('div', { class: 'au-muted', text: 'This case has not been part of a run yet.' }));
                }
                openDialog('Test case', wrap);
            }).catch(function (e) { showMessage(e.message, 'error'); });
        }

        function openResult(runId, resultId) {
            api('GET', '/runs/' + runId + '/results/' + resultId).then(function (detail) {
                var wrap = el('div', { class: 'au-stack', style: '--au-gap:12px' }, [
                    el('div', null, [el('span', { class: 'au-eyebrow', text: 'Question' }), el('div', { class: 'bm-block', text: detail.result.question })]),
                    analysisNode(detail)
                ]);
                openDialog('Case analysis', wrap);
            }).catch(function (e) { showMessage(e.message, 'error'); });
        }

        /** One row of a test-case table. handlers: open(c), toggleReviewed(c), remove(c). */
        function caseRow(c, handlers) {
            var last = c.lastResult;
            return el('tr', null, [
                el('td', { class: 'bm-question' }, [
                    el('a', { href: '#', class: 'au-link', title: c.question, text: c.question, onclick: function (e) { e.preventDefault(); handlers.open(c); } })
                ]),
                el('td', { class: 'bm-clip' }, [
                    el('div', { class: 'au-ellipsis au-strong', text: c.expectedDocumentTitle || '—' }),
                    el('div', { class: 'au-ellipsis au-caption', title: c.expectedChunkPreview || '', text: c.expectedChunkPreview || '' })
                ]),
                el('td', null, [reviewedChip(c.isReviewed)]),
                el('td', null, [
                    c.isStale
                        ? chip('au-chip--outlined', 'Stale', c.staleExplanation)
                        : (last ? badgeFor(last.classification, last.expectedChunkRank) : el('span', { class: 'au-faint', text: 'Not run' }))
                ]),
                el('td', null, [el('div', { class: 'au-row' }, [
                    el('button', { type: 'button', class: 'au-btn au-btn--outlined au-btn--sm', text: 'Open', onclick: function () { handlers.open(c); } }),
                    el('button', { type: 'button', class: 'au-btn au-btn--outlined au-btn--sm', text: c.isReviewed ? 'Unreview' : 'Mark reviewed', onclick: function () { handlers.toggleReviewed(c); } }),
                    el('button', { type: 'button', class: 'au-btn au-btn--danger-outlined au-btn--sm', text: 'Delete', onclick: function () { handlers.remove(c); } })
                ])])
            ]);
        }

        /** Previous/next pager under a case table. */
        function renderPager(prefix, skip, total, pageSize) {
            $(prefix + '-cases-pager').hidden = total <= pageSize;
            var from = total === 0 ? 0 : skip + 1;
            var to = Math.min(skip + pageSize, total);
            $(prefix + '-cases-range').textContent = from + '–' + to + ' of ' + total;
            $(prefix + '-cases-prev').disabled = skip === 0;
            $(prefix + '-cases-next').disabled = skip + pageSize >= total;
        }

        return {
            api: api, showMessage: showMessage, openDialog: openDialog, analysisNode: analysisNode, openCase: openCase,
            openResult: openResult, caseRow: caseRow, renderPager: renderPager
        };
    }

    window.BenchmarkKit = {
        create: create, $: $, el: el, pct: pct, num: num, fmtDate: fmtDate, shortId: shortId, chip: chip, badgeFor: badgeFor,
        generationChip: generationChip, settingsLine: settingsLine, statTile: statTile, statGrid: statGrid, emptyRow: emptyRow,
        cell: cell, ask: ask
    };
})();
