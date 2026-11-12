// Omni Admin — runs in <head>, before the first paint: stamps the saved (or system) theme and, on signed-in pages, the
// collapsed sidebar, so dark mode has no white flash. A file rather than an inline script, so the Content-Security-Policy
// can refuse every inline script (QA t6-admin-15). Falls back once to the pre-Aurora keys (sf-theme, sf-sidebar).
(function () {
    try {
        var ls = localStorage, r = document.documentElement;
        var saved = ls.getItem('admin.themeMode') || ls.getItem('sf-theme');
        var mode = saved === 'light' || saved === 'dark' ? saved
            : (window.matchMedia && matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
        r.setAttribute('data-theme', mode);
        r.style.backgroundColor = mode === 'dark' ? '#070b15' : '#f2f5fa';
        if (document.currentScript && document.currentScript.hasAttribute('data-rail')) {
            var rail = ls.getItem('admin.sidebarCollapsed');
            if (rail === '1' || (rail === null && ls.getItem('sf-sidebar') === 'collapsed')) r.classList.add('au-side-collapsed');
        }
    } catch (e) { }
})();
