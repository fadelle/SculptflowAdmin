// Sign in: show-password button, Caps Lock warning, and a one-time submit ("Signing in…").
(function () {
  var toggle = document.querySelector('[data-pw-toggle]');
  if (toggle) {
    toggle.addEventListener('click', function () {
      var input = toggle.parentElement.querySelector('input');
      var show = input.type === 'password';
      input.type = show ? 'text' : 'password';
      toggle.textContent = show ? '🙈' : '👁';
      toggle.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
      toggle.title = toggle.getAttribute('aria-label');
      input.focus();
    });
  }

  var caps = document.querySelector('[data-caps]');
  var pw = document.querySelector('[data-pw]');
  if (caps && pw) {
    ['keydown', 'keyup'].forEach(function (t) {
      pw.addEventListener(t, function (e) {
        if (e.getModifierState) caps.hidden = !e.getModifierState('CapsLock');
      });
    });
    pw.addEventListener('blur', function () { caps.hidden = true; });
  }

  var form = document.querySelector('[data-login-form]');
  if (form) {
    form.addEventListener('submit', function (e) {
      if (form.dataset.sent) { e.preventDefault(); return; }
      form.dataset.sent = '1';
      var btn = form.querySelector('[data-busy-label]');
      if (btn) {
        btn.disabled = true;
        btn.innerHTML = '<span class="login-spin"></span>' + btn.getAttribute('data-busy-label');
      }
    });
  }
})();
