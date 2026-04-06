function fmtMoney(s) {
    s = (s || '').replace(/[^0-9]/g, '').replace(/^0+/, '');
    if (!s) return '0';
    var r = '';
    for (var i = s.length - 1, c = 0; i >= 0; i--, c++) {
        if (c > 0 && c % 3 === 0) r = '.' + r;
        r = s[i] + r;
    }
    return r;
}

function rawMoney(s) {
    return (s || '').replace(/[^0-9]/g, '').replace(/^0+/, '') || '0';
}

function initMoneyInput(el) {
    if (el.dataset.moneyInit === '1') return;
    el.dataset.moneyInit = '1';

    var fieldName = el.getAttribute('name');
    if (!fieldName) return;

    el.removeAttribute('name');

    var hidden = document.createElement('input');
    hidden.type = 'hidden';
    hidden.name = fieldName;
    hidden.value = rawMoney(el.value);
    el.after(hidden);

    el._moneyHidden = hidden;
    el.value = fmtMoney(el.value);

    el.addEventListener('input', function () {
        var digits = this.value.replace(/[^0-9]/g, '');
        if (digits.length > 15) digits = digits.substring(0, 15);
        hidden.value = digits.replace(/^0+/, '') || '0';

        var sel = this.selectionStart;
        var oldLen = this.value.length;
        this.value = fmtMoney(digits);
        var newLen = this.value.length;
        var newPos = Math.max(0, sel + (newLen - oldLen));
        this.setSelectionRange(newPos, newPos);
    });

    el.addEventListener('paste', function (e) {
        e.preventDefault();
        var pasted = (e.clipboardData || window.clipboardData).getData('text');
        var pastedDigits = pasted.replace(/[^0-9]/g, '');
        if (!pastedDigits) return;
        var before = this.value.substring(0, this.selectionStart).replace(/[^0-9]/g, '');
        var after = this.value.substring(this.selectionEnd).replace(/[^0-9]/g, '');
        var digits = before + pastedDigits + after;
        if (digits.length > 15) digits = digits.substring(0, 15);
        hidden.value = digits.replace(/^0+/, '') || '0';
        this.value = fmtMoney(digits);
        var cursorDigits = before.length + pastedDigits.length;
        var pos = 0, dc = 0;
        for (var i = 0; i < this.value.length; i++) {
            if (this.value[i] !== '.') { dc++; if (dc === cursorDigits) { pos = i + 1; break; } }
        }
        if (dc < cursorDigits) pos = this.value.length;
        this.setSelectionRange(pos, pos);
    });
}

document.addEventListener('submit', function (e) {
    var form = e.target;
    if (!form || !form.querySelectorAll) return;
    form.querySelectorAll('.money-input[data-money-init="1"]').forEach(function (el) {
        if (el._moneyHidden) {
            el._moneyHidden.value = rawMoney(el.value);
        }
    });
}, true);

function previewImage(input) {
    if (!input.files || !input.files[0]) return;
    var previewId = input.dataset.imgPreview;
    if (!previewId) return;
    var img = document.getElementById(previewId);
    if (!img) return;
    var reader = new FileReader();
    reader.onload = function (e) { img.src = e.target.result; };
    reader.readAsDataURL(input.files[0]);
}

function paginationSearch(event, form, page) {
    if (event) event.preventDefault();
    if (!form) return;

    var url = form.action;
    var method = (form.method || "GET").toUpperCase();
    var targetId = form.dataset.target;

    var formData = new FormData(form);
    formData.append("page", page);

    var fetchUrl = url;
    if (method === "GET") {
        var params = new URLSearchParams(formData).toString();
        fetchUrl = url + "?" + params;
    }

    var targetEl = null;
    if (targetId) {
        targetEl = document.getElementById(targetId);
        if (targetEl) {
            targetEl.innerHTML = '<div class="text-center py-4"><span>Đang tải dữ liệu...</span></div>';
        }
    }

    fetch(fetchUrl, {
        method: method,
        body: method === "GET" ? null : formData
    })
    .then(function (res) { return res.text(); })
    .then(function (html) {
        if (targetEl) targetEl.innerHTML = html;
    })
    .catch(function () {
        if (targetEl) targetEl.innerHTML = '<div class="text-danger">Không tải được dữ liệu</div>';
    });
}

(function () {
    var modalEl = document.getElementById("dialogModal");
    if (!modalEl) return;

    var modalContent = modalEl.querySelector(".modal-content");

    modalEl.addEventListener('hidden.bs.modal', function () {
        modalContent.innerHTML = '';
    });

    window.openModal = function (event, link) {
        if (!link) return;
        if (event) event.preventDefault();

        var url = link.getAttribute("href");

        modalContent.innerHTML = '<div class="modal-body text-center py-5"><span>Đang tải dữ liệu...</span></div>';

        var modal = bootstrap.Modal.getInstance(modalEl);
        if (!modal) {
            modal = new bootstrap.Modal(modalEl, { backdrop: 'static', keyboard: false });
        }

        modal.show();

        fetch(url)
            .then(function (res) { return res.text(); })
            .then(function (html) { modalContent.innerHTML = html; })
            .catch(function () {
                modalContent.innerHTML = '<div class="modal-body text-danger">Không tải được dữ liệu</div>';
            });
    };
})();
