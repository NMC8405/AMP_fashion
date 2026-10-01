// =========================================================
// AMP FASHION STORE — Client-side helpers (vanilla JS, không phụ thuộc thư viện ngoài)
// =========================================================
(function () {
  "use strict";

  document.addEventListener("DOMContentLoaded", function () {
    initMobileMenu();
    initSearchToggle();
    initOtpBoxes();
    initOtpCountdown();
    initPasswordStrength();
    initQtyBoxes();
    initStarPicker();
    initPayOptions();
    initAdminSidebar();
    initAutoDismissAlerts();
    initWishlistToggle();
    initNewsletter();
  });

  // ---------- Mobile nav ----------
  function initMobileMenu() {
    var toggle = document.querySelector(".amp-mobile-toggle");
    var nav = document.querySelector(".amp-nav");
    if (!toggle || !nav) return;
    toggle.addEventListener("click", function () {
      nav.classList.toggle("amp-nav-open");
      if (nav.classList.contains("amp-nav-open")) {
        nav.style.display = "flex";
        nav.style.flexDirection = "column";
        nav.style.position = "absolute";
        nav.style.top = "64px";
        nav.style.left = "0";
        nav.style.right = "0";
        nav.style.background = "#fff";
        nav.style.padding = "16px 24px";
        nav.style.borderBottom = "1px solid #e3e1dc";
        nav.style.gap = "4px";
        nav.style.zIndex = "999";
      } else {
        nav.removeAttribute("style");
      }
    });
  }

  // ---------- Search bar toggle ----------
  function initSearchToggle() {
    var btn = document.querySelector("[data-toggle-search]");
    var bar = document.querySelector(".amp-search-bar");
    if (!btn || !bar) return;
    btn.addEventListener("click", function () {
      bar.classList.toggle("show");
      if (bar.classList.contains("show")) {
        var input = bar.querySelector("input");
        if (input) input.focus();
      }
    });
  }

  // ---------- OTP boxes: 6 ô tự động nhảy sang ô kế tiếp ----------
  function initOtpBoxes() {
    var wrap = document.querySelector("[data-otp-wrap]");
    if (!wrap) return;
    var boxes = Array.prototype.slice.call(wrap.querySelectorAll(".amp-otp-box"));
    var hidden = document.querySelector("[data-otp-hidden]");

    function syncHidden() {
      if (hidden) hidden.value = boxes.map(function (b) { return b.value; }).join("");
    }

    boxes.forEach(function (box, idx) {
      box.addEventListener("input", function () {
        box.value = box.value.replace(/[^0-9]/g, "").slice(0, 1);
        if (box.value && idx < boxes.length - 1) boxes[idx + 1].focus();
        syncHidden();
      });
      box.addEventListener("keydown", function (e) {
        if (e.key === "Backspace" && !box.value && idx > 0) {
          boxes[idx - 1].focus();
        }
      });
      box.addEventListener("paste", function (e) {
        var text = (e.clipboardData || window.clipboardData).getData("text").replace(/[^0-9]/g, "");
        if (!text) return;
        e.preventDefault();
        for (var i = 0; i < boxes.length; i++) boxes[i].value = text[i] || "";
        syncHidden();
        var lastFilled = Math.min(text.length, boxes.length) - 1;
        if (lastFilled >= 0) boxes[lastFilled].focus();
      });
    });

    var form = wrap.closest("form");
    if (form) form.addEventListener("submit", syncHidden);
  }

  // ---------- Đếm ngược gửi lại OTP ----------
  function initOtpCountdown() {
    var el = document.querySelector("[data-otp-countdown]");
    if (!el) return;
    var seconds = parseInt(el.getAttribute("data-otp-countdown"), 10) || 60;
    var resendBtn = document.querySelector("[data-otp-resend]");
    var timerLabel = document.querySelector("[data-otp-timer-text]");
    if (resendBtn) resendBtn.style.display = "none";

    var timer = setInterval(function () {
      seconds--;
      if (timerLabel) timerLabel.textContent = "Gửi lại mã sau " + seconds + "s";
      if (seconds <= 0) {
        clearInterval(timer);
        if (timerLabel) timerLabel.style.display = "none";
        if (resendBtn) resendBtn.style.display = "inline";
      }
    }, 1000);
  }

  // ---------- Độ mạnh mật khẩu ----------
  function initPasswordStrength() {
    var input = document.querySelector("[data-pwd-strength]");
    var bar = document.querySelector("[data-strength-bar]");
    var hint = document.querySelector("[data-strength-hint]");
    if (!input || !bar) return;
    var fill = bar.querySelector("span");

    input.addEventListener("input", function () {
      var v = input.value;
      var score = 0;
      if (v.length >= 6) score++;
      if (v.length >= 10) score++;
      if (/[A-Z]/.test(v) && /[a-z]/.test(v)) score++;
      if (/[0-9]/.test(v)) score++;
      if (/[^A-Za-z0-9]/.test(v)) score++;

      var pct = Math.min(100, (score / 5) * 100);
      var color = "#a8384a";
      var label = "Yếu";
      if (score >= 4) { color = "#2f7d4f"; label = "Mạnh"; }
      else if (score >= 2) { color = "#dd9d2b"; label = "Trung bình"; }

      if (fill) { fill.style.width = pct + "%"; fill.style.background = color; }
      if (hint) hint.textContent = v ? "Độ mạnh mật khẩu: " + label : "";
    });
  }

  // ---------- Bộ đếm số lượng (giỏ hàng / chi tiết sản phẩm) ----------
  function initQtyBoxes() {
    document.querySelectorAll(".amp-qty-box").forEach(function (box) {
      var input = box.querySelector("input");
      var minus = box.querySelector("[data-qty-minus]");
      var plus = box.querySelector("[data-qty-plus]");
      var max = parseInt(input.getAttribute("max") || "999", 10);
      var min = parseInt(input.getAttribute("min") || "1", 10);

      function clamp() {
        var v = parseInt(input.value, 10);
        if (isNaN(v)) v = min;
        if (v < min) v = min;
        if (v > max) v = max;
        input.value = v;
      }

      if (minus) minus.addEventListener("click", function () {
        input.value = (parseInt(input.value, 10) || min) - 1;
        clamp();
        if (box.hasAttribute("data-auto-submit")) box.closest("form").submit();
      });
      if (plus) plus.addEventListener("click", function () {
        input.value = (parseInt(input.value, 10) || min) + 1;
        clamp();
        if (box.hasAttribute("data-auto-submit")) box.closest("form").submit();
      });
      input.addEventListener("change", function () {
        clamp();
        if (box.hasAttribute("data-auto-submit")) box.closest("form").submit();
      });
    });
  }

  // ---------- Chọn số sao đánh giá (hỗ trợ nhiều picker trên 1 trang, vd nhiều modal đánh giá) ----------
  function initStarPicker() {
    document.querySelectorAll("[data-star-picker]").forEach(function (picker) {
      var stars = Array.prototype.slice.call(picker.querySelectorAll(".s"));
      var hidden = picker.querySelector("[data-star-value]");

      function paint(value) {
        stars.forEach(function (s, i) {
          s.classList.toggle("on", i < value);
        });
      }

      stars.forEach(function (s, idx) {
        s.addEventListener("click", function () {
          if (hidden) hidden.value = idx + 1;
          paint(idx + 1);
        });
        s.addEventListener("mouseenter", function () { paint(idx + 1); });
      });
      picker.addEventListener("mouseleave", function () {
        paint(parseInt(hidden ? hidden.value : 5, 10));
      });
      paint(parseInt(hidden ? hidden.value : 5, 10));
    });
  }

  // ---------- Chọn phương thức thanh toán ----------
  function initPayOptions() {
    var options = document.querySelectorAll(".amp-pay-option");
    if (!options.length) return;
    options.forEach(function (opt) {
      var radio = opt.querySelector('input[type="radio"]');
      if (!radio) return;
      function refresh() {
        options.forEach(function (o) { o.classList.remove("selected"); });
        if (radio.checked) opt.classList.add("selected");
        var qr = opt.parentElement.querySelector("[data-qr-target]");
      }
      radio.addEventListener("change", function () {
        options.forEach(function (o) { o.classList.remove("selected"); });
        opt.classList.add("selected");
        document.querySelectorAll("[data-bank-block]").forEach(function (b) {
          b.style.display = (radio.value === "ChuyenKhoanNganHang" && radio.checked) ? "block" : "none";
        });
      });
      refresh();
    });
  }

  // ---------- Sidebar quản trị (mobile) ----------
  function initAdminSidebar() {
    var toggle = document.querySelector("[data-admin-toggle]");
    var sidebar = document.querySelector(".amp-admin-sidebar");
    if (!toggle || !sidebar) return;
    toggle.addEventListener("click", function () { sidebar.classList.toggle("open"); });
    document.addEventListener("click", function (e) {
      if (sidebar.classList.contains("open") && !sidebar.contains(e.target) && e.target !== toggle) {
        sidebar.classList.remove("open");
      }
    });
  }

  // ---------- Tự ẩn thông báo sau vài giây ----------
  function initAutoDismissAlerts() {
    document.querySelectorAll("[data-auto-dismiss]").forEach(function (el) {
      setTimeout(function () {
        el.style.transition = "opacity .4s ease";
        el.style.opacity = "0";
        setTimeout(function () { el.remove(); }, 400);
      }, 5000);
    });
  }

  // ---------- Yêu thích sản phẩm (AJAX toggle) ----------
  function initWishlistToggle() {
    document.addEventListener("submit", function (e) {
      var form = e.target;
      if (!form.classList.contains("amp-wish-form")) return;
      e.preventDefault();

      var btn = form.querySelector(".amp-wish-btn");
      var formData = new FormData(form);

      fetch(form.action, {
        method: "POST",
        body: formData,
        headers: {
          "X-Requested-With": "XMLHttpRequest",
          "Accept": "application/json"
        }
      })
      .then(function (res) {
        if (res.redirected) {
          window.location.href = res.url;
          return;
        }
        return res.json();
      })
      .then(function (data) {
        if (!data || !data.success) return;
        var daYeuThich = data.daYeuThich;
        if (btn) {
          btn.classList.toggle("active", daYeuThich);
          btn.title = daYeuThich ? "Bỏ yêu thích" : "Yêu thích";
          var icon = btn.querySelector("i");
          if (icon) {
            icon.className = "bi " + (daYeuThich ? "bi-heart-fill" : "bi-heart");
          }
        }
        if (daYeuThich) {
          form.action = "/YeuThich/Xoa";
        } else {
          form.action = "/YeuThich/Them";
        }

        var wishBadge = document.querySelector('a[href*="YeuThich"] .amp-icon-badge');
        if (data.count > 0) {
          if (!wishBadge) {
            var wishLink = document.querySelector('a[href*="YeuThich"]');
            if (wishLink) {
              wishBadge = document.createElement("span");
              wishBadge.className = "amp-icon-badge";
              wishLink.appendChild(wishBadge);
            }
          }
          if (wishBadge) wishBadge.textContent = data.count;
        } else if (wishBadge) {
          wishBadge.remove();
        }
      })
      .catch(function () {
        form.submit();
      });
    });
  }

  // ---------- Đăng ký nhận tin ưu đãi ----------
  function initNewsletter() {
    var section = document.getElementById("section-newsletter");
    if (!section) return;

    if (localStorage.getItem("amp_newsletter_subscribed") === "true") {
      section.style.display = "none";
      return;
    }

    var form = section.querySelector("form");
    if (!form) return;

    form.addEventListener("submit", function (e) {
      e.preventDefault();
      var input = form.querySelector('input[type="email"]') || form.querySelector("input");
      if (!input) return;
      var email = input.value.trim();
      var emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
      if (!emailRegex.test(email)) {
        alert("Vui lòng nhập đúng định dạng email (ví dụ: example@gmail.com).");
        input.focus();
        return;
      }

      localStorage.setItem("amp_newsletter_subscribed", "true");

      var card = section.querySelector(".amp-surface-card");
      if (card) {
        card.style.borderColor = "#86efac";
        card.style.background = "#f0fdf4";
        card.innerHTML =
          '<div style="font-size:42px;color:#16a34a;margin-bottom:12px"><i class="bi bi-check-circle-fill"></i></div>' +
          '<h3 style="font-size:20px;font-weight:700;color:#166534;margin-bottom:8px">Đăng ký nhận tin thành công!</h3>' +
          '<p style="font-size:14px;color:#15803d;max-width:480px;margin:0 auto">Cảm ơn bạn đã đăng ký. AMP sẽ gửi thông tin bộ sưu tập mới và ưu đãi độc quyền sớm nhất đến <strong>' +
          escapeHtml(email) +
          '</strong>.</p>';
        setTimeout(function () {
          card.style.transition = "opacity .6s ease, transform .6s ease";
          card.style.opacity = "0";
          card.style.transform = "translateY(-10px)";
          setTimeout(function () { section.style.display = "none"; }, 600);
        }, 4000);
      }
    });
  }

  function escapeHtml(str) {
    return str.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
  }
})();
