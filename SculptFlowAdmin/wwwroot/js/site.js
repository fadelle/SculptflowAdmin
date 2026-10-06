// Shows every <time data-utc="..."> in the viewer's own time zone (the DB stores UTC; activity is shown in viewer time).
// Elements with data-tz="Area/City" are shown in that zone instead (appointments use the clinic's time).
(function () {
  function fmt(el) {
    var iso = el.getAttribute('data-utc');
    if (!iso) return;
    var d = new Date(iso);
    if (isNaN(d)) return;
    var opts = { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    var tz = el.getAttribute('data-tz');
    if (tz) opts.timeZone = tz;
    try { el.textContent = d.toLocaleString(undefined, opts) + (tz ? ' (' + tz + ')' : ''); }
    catch (e) { el.textContent = d.toISOString(); }
    el.title = d.toISOString();
  }
  document.querySelectorAll('time[data-utc]').forEach(fmt);

  // Buttons with data-confirm ask before submitting their form.
  document.addEventListener('submit', function (e) {
    var btn = e.submitter;
    var msg = (btn && btn.getAttribute('data-confirm')) || e.target.getAttribute('data-confirm');
    if (msg && !window.confirm(msg)) e.preventDefault();
  });
})();
