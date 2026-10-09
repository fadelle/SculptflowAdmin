// SculptFlow Admin — page behaviours on signed-in pages. Plain JS, no libraries. The shell (sidebar, theme, header menus,
// Ctrl+K, toasts) is aurora.js; this file keeps the pages' own hooks:
//  - <time data-utc> in the viewer's time zone (data-tz="Area/City" shows that zone instead; appointments use clinic time)
//  - popups: [data-open="dialogId"] opens <dialog id> (data-autoopen opens it on load), [data-close] or a backdrop click closes it
//  - data-confirm on a button or form asks in a small popup (#confirm-dialog in _Layout) before submitting
//  - tables: click a header to sort the rows on the page; <input data-filter="#tableId"> filters them as you type
//  - filter bars (form.filters) submit as soon as a select or checkbox changes
//  - [data-copy="#id"] copies that element's text; [data-dismiss-alert] closes its alert; submit buttons show a spinner
//    and can't be double-clicked
(function () {
  var doc = document;
  function $(sel, el) { return (el || doc).querySelector(sel); }
  function $$(sel, el) { return Array.prototype.slice.call((el || doc).querySelectorAll(sel)); }

  // ---- Times ----
  $$('time[data-utc]').forEach(function (el) {
    var d = new Date(el.getAttribute('data-utc'));
    if (isNaN(d)) return;
    var opts = { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    var tz = el.getAttribute('data-tz');
    if (tz) opts.timeZone = tz;
    try { el.textContent = d.toLocaleString(undefined, opts) + (tz ? ' (' + tz + ')' : ''); }
    catch (e) { el.textContent = d.toISOString(); }
    el.title = d.toISOString();
  });

  // ---- Popups ----
  doc.addEventListener('click', function (e) {
    var opener = e.target.closest('[data-open]');
    if (opener) {
      var dlg = doc.getElementById(opener.getAttribute('data-open'));
      if (dlg && dlg.showModal) {
        e.preventDefault();
        dlg.showModal();
        var first = $('input:not([type=hidden]):not([disabled]), select, textarea', dlg);
        if (first) first.focus();
      }
      return;
    }
    var closer = e.target.closest('[data-close]');
    if (closer) { e.preventDefault(); closer.closest('dialog').close(); return; }
    // A click on the dimmed area around a popup closes it.
    if (e.target.tagName === 'DIALOG' && e.target.open) {
      var r = e.target.getBoundingClientRect();
      if (e.clientX < r.left || e.clientX > r.right || e.clientY < r.top || e.clientY > r.bottom) e.target.close();
    }
  });

  // A popup marked data-autoopen opens with the page (e.g. Plans?edit=code).
  $$('dialog[data-autoopen]').forEach(function (d) { if (d.showModal) d.showModal(); });

  // ---- Confirm + busy submit ----
  var confirmDlg = $('#confirm-dialog');
  // A button's visible words, without its icon's ligature name (<span class="au-icon">block</span>Deactivate → "Deactivate").
  function labelOf(btn) {
    var copy = btn.cloneNode(true);
    $$('.au-icon', copy).forEach(function (i) { i.remove(); });
    return copy.textContent.trim();
  }
  doc.addEventListener('submit', function (e) {
    var form = e.target, btn = e.submitter;
    if (form.method === 'dialog') return;
    if (form.dataset.submitting) { e.preventDefault(); return; }
    var msg = (btn && btn.getAttribute('data-confirm')) || form.getAttribute('data-confirm');
    if (msg && !form.dataset.confirmed) {
      e.preventDefault();
      if (!confirmDlg) { if (window.confirm(msg)) { form.dataset.confirmed = '1'; form.requestSubmit(btn); } return; }
      $('#confirm-text').textContent = msg;
      var danger = btn && (btn.classList.contains('danger') || /(^|\s)au-btn--danger/.test(btn.className));
      $('#confirm-ok').className = 'au-btn ' + (danger ? 'au-btn--danger' : 'au-btn--primary');
      $('#confirm-ok').textContent = (btn && labelOf(btn)) || 'Confirm';
      confirmDlg.returnValue = '';
      confirmDlg.onclose = function () {
        if (confirmDlg.returnValue === 'ok') { form.dataset.confirmed = '1'; form.requestSubmit(btn); }
      };
      confirmDlg.showModal();
      return;
    }
    delete form.dataset.confirmed;
    if (form.method.toLowerCase() === 'post') {
      form.dataset.submitting = '1';
      if (btn) btn.classList.add('is-busy');
    }
  });
  // Back/forward cache: a restored page must be usable again.
  window.addEventListener('pageshow', function () {
    $$('form[data-submitting]').forEach(function (f) { delete f.dataset.submitting; });
    $$('.is-busy').forEach(function (b) { b.classList.remove('is-busy'); });
  });

  // ---- Filter bars ----
  $$('form.filters').forEach(function (form) {
    form.addEventListener('change', function (e) {
      if (e.target.matches('select, input[type=checkbox]')) form.requestSubmit();
    });
  });

  // ---- Tables: sort + quick filter ----
  function cellKey(td) {
    if (!td) return '';
    if (td.hasAttribute('data-sort')) return td.getAttribute('data-sort');
    var t = td.querySelector('time[data-utc]');
    if (t) return t.getAttribute('data-utc');
    return td.textContent.trim();
  }
  function asNumber(s) {
    var n = s.replace(/[,\s$€£%]|USD|EUR/g, '');
    return /^[-+]?\d*\.?\d+$/.test(n) ? parseFloat(n) : NaN;
  }
  function bodyRows(table) {
    return $$('tr', table).filter(function (tr) { return !tr.querySelector('th') && !tr.classList.contains('no-match'); });
  }
  $$('table').forEach(function (table) {
    if (table.hasAttribute('data-nosort')) return;
    var head = $$('tr', table).find(function (tr) { return tr.querySelector('th'); });
    if (!head || bodyRows(table).length < 2) return;
    $$('th', head).forEach(function (th, i) {
      if (!th.textContent.trim()) return;
      th.classList.add('sortable');
      th.tabIndex = 0;
      function sort() {
        var dir = th.getAttribute('aria-sort') === 'ascending' ? 'descending' : 'ascending';
        $$('th', head).forEach(function (h) { h.removeAttribute('aria-sort'); });
        th.setAttribute('aria-sort', dir);
        var rows = bodyRows(table), parent = rows[0].parentNode;
        var keys = rows.map(function (r) { return cellKey(r.children[i]); });
        var numeric = keys.every(function (k) { return k === '' || k === '—' || !isNaN(asNumber(k)); });
        var order = rows.map(function (r, j) { return j; }).sort(function (a, b) {
          var x = keys[a], y = keys[b], c;
          if (numeric) c = (asNumber(x) || 0) - (asNumber(y) || 0);
          else c = x.localeCompare(y, undefined, { numeric: true, sensitivity: 'base' });
          return dir === 'ascending' ? c : -c;
        });
        order.forEach(function (j) { parent.appendChild(rows[j]); });
        // Totals rows (th cells under the header) stay at the bottom.
        $$('tr', parent).forEach(function (tr) { if (tr !== head && tr.querySelector('th')) parent.appendChild(tr); });
      }
      th.addEventListener('click', sort);
      th.addEventListener('keydown', function (e) { if (e.key === 'Enter') sort(); });
    });
  });
  $$('input[data-filter]').forEach(function (input) {
    var table = $(input.getAttribute('data-filter'));
    if (!table) return;
    var none = doc.createElement('tr');
    none.className = 'no-match';
    none.hidden = true;
    none.innerHTML = '<td colspan="99">Nothing matches “<span></span>”.</td>';
    input.addEventListener('keydown', function (e) { if (e.key === 'Enter') e.preventDefault(); });
    var rows = bodyRows(table);
    if (rows.length) rows[0].parentNode.appendChild(none);
    input.addEventListener('input', function () {
      var q = input.value.trim().toLowerCase(), shown = 0;
      rows.forEach(function (r) {
        var hit = !q || r.textContent.toLowerCase().indexOf(q) >= 0;
        r.hidden = !hit;
        if (hit) shown++;
      });
      none.hidden = shown > 0;
      none.querySelector('span').textContent = input.value.trim();
    });
  });

  // ---- Copy buttons: data-copy="#id" copies that element's text ----
  doc.addEventListener('click', function (e) {
    var b = e.target.closest('[data-copy]');
    if (!b || !navigator.clipboard) return;
    var el = $(b.getAttribute('data-copy'));
    navigator.clipboard.writeText(el ? el.textContent.trim() : '').then(function () {
      var t = b.textContent; b.textContent = 'Copied'; setTimeout(function () { b.textContent = t; }, 1500);
    });
  });

  // ---- Dismissable alerts (e.g. a failed save in _Layout) ----
  doc.addEventListener('click', function (e) {
    var b = e.target.closest('[data-dismiss-alert]');
    if (b) b.closest('.au-alert').remove();
  });
})();
