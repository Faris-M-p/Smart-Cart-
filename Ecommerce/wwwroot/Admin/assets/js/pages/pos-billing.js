(function () {
    const STORAGE_KEY = 'smartcart.pos.drafts.v1';
    const API = {
        getList: '/Admin/Sale/GetSaleList',
        getById: '/Admin/Sale/GetById',
        create: '/Admin/Sale/Create',
        update: '/Admin/Sale/Update',
        delete: '/Admin/Sale/Delete',
        searchSkus: '/Admin/Sale/SearchSkus',
        searchCustomers: '/Admin/Sale/SearchCustomers',
        adjacent: '/Admin/Sale/Adjacent'
    };

    let drafts = [];
    let current = null;
    let skuTimer = null;
    let customerTimer = null;
    let skuResults = [];
    let skuIndex = 0;
    let historyPage = 1;
    let historyPageSize = 10;
    let historyPager = null;
    let saving = false;

    document.addEventListener('DOMContentLoaded', init);

    function init() {
        document.body.classList.add('pos-page');
        drafts = loadDrafts();
        if (!drafts.length) {
            drafts.push(createDraft());
        }
        current = drafts[0];
        bindEvents();
        historyPager = new Pagination({
            containerId: 'posHistoryPagination',
            infoId: 'posHistoryInfo',
            onPageChange: function (page) { loadHistory(page); }
        });
        document.getElementById('posHistoryOffcanvas')?.addEventListener('shown.bs.offcanvas', function () {
            loadHistory(1);
        });
        document.getElementById('posHistorySearch')?.addEventListener('input', function () {
            clearTimeout(window.__posHistoryTimer);
            window.__posHistoryTimer = setTimeout(function () { loadHistory(1); }, 350);
        });
        document.getElementById('posCreateCustomerForm')?.addEventListener('submit', function (e) {
            e.preventDefault();
            createCustomerFromPopup();
        });
        renderAll();
        focusProductSearch();
    }

    function bindEvents() {
        const skuInput = document.getElementById('posSkuSearch');
        skuInput.addEventListener('input', function () {
            clearTimeout(skuTimer);
            skuTimer = setTimeout(function () { searchSkus(skuInput.value); }, 220);
        });
        skuInput.addEventListener('keydown', onSkuKeyDown);

        document.getElementById('posCustomerSearch').addEventListener('input', function (e) {
            clearTimeout(customerTimer);
            customerTimer = setTimeout(function () { searchCustomers(e.target.value); }, 250);
        });

        document.getElementById('posBillSearch').addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                searchBill(e.target.value);
            }
        });

        document.getElementById('posTendered').addEventListener('input', renderPaymentExtras);
        document.getElementById('posNotes').addEventListener('input', function (e) {
            current.notes = e.target.value;
            persistDrafts();
        });

        document.addEventListener('keydown', onGlobalKey);
        document.addEventListener('click', function (e) {
            if (!e.target.closest('.pos-search')) closeSkuResults();
            if (!e.target.closest('.pos-customer-box')) closeCustomerResults();
        });
    }

    function onGlobalKey(e) {
        const inField = /^(INPUT|TEXTAREA|SELECT)$/.test(e.target.tagName);
        if (e.key === 'F2') {
            e.preventDefault();
            focusProductSearch();
            return;
        }
        if (e.key === 'F10') {
            e.preventDefault();
            newBill();
            return;
        }
        if (e.key === 'F8') {
            e.preventDefault();
            document.getElementById('posCustomerSearch').focus();
            return;
        }
        if (e.key === 'F9' || (e.key === 'Enter' && e.ctrlKey)) {
            e.preventDefault();
            completeBill();
            return;
        }
        if (!inField && (e.key === 'ArrowLeft' || e.key === 'ArrowRight')) {
            e.preventDefault();
            navigateBill(e.key === 'ArrowLeft' ? -1 : 1);
        }
    }

    function onSkuKeyDown(e) {
        const open = document.getElementById('posSkuResults').classList.contains('is-open');
        if (e.key === 'ArrowDown' && open) {
            e.preventDefault();
            skuIndex = Math.min(skuResults.length - 1, skuIndex + 1);
            renderSkuResults();
        } else if (e.key === 'ArrowUp' && open) {
            e.preventDefault();
            skuIndex = Math.max(0, skuIndex - 1);
            renderSkuResults();
        } else if (e.key === 'Enter') {
            e.preventDefault();
            if (open && skuResults[skuIndex]) addSku(skuResults[skuIndex]);
        } else if (e.key === 'Escape') {
            closeSkuResults();
        }
    }

    function createDraft() {
        return {
            localId: 'd-' + Date.now() + '-' + Math.random().toString(36).slice(2, 6),
            saleId: 0,
            status: 'pending',
            invoiceLabel: '',
            hasReturns: false,
            saleDate: todayIso(),
            items: [],
            customer: null,
            payment: 'Cash',
            notes: '',
            tendered: ''
        };
    }

    function loadDrafts() {
        try {
            const raw = sessionStorage.getItem(STORAGE_KEY);
            const parsed = raw ? JSON.parse(raw) : [];
            return Array.isArray(parsed) ? parsed.filter(function (b) { return b && b.status === 'pending'; }) : [];
        } catch (e) {
            return [];
        }
    }

    function persistDrafts() {
        const pending = drafts.filter(function (b) { return b.status === 'pending'; });
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(pending));
    }

    function todayIso() {
        return new Date().toISOString().slice(0, 10);
    }

    function newBill() {
        if (current && current.status === 'pending') {
            const idx = drafts.findIndex(function (b) { return b.localId === current.localId; });
            if (idx >= 0) drafts[idx] = current;
        }
        const draft = createDraft();
        drafts.push(draft);
        current = draft;
        persistDrafts();
        renderAll();
        focusProductSearch();
    }

    function selectDraft(localId) {
        const next = drafts.find(function (b) { return b.localId === localId; });
        if (!next) return;
        current = next;
        renderAll();
        focusProductSearch();
    }

    async function navigateBill(dir) {
        if (current.status === 'pending') {
            const pending = drafts.filter(function (b) { return b.status === 'pending'; });
            const idx = pending.findIndex(function (b) { return b.localId === current.localId; });
            const next = pending[idx + dir];
            if (next) selectDraft(next.localId);
            return;
        }
        if (!current.saleId) return;
        try {
            const response = await fetch(API.adjacent + '/' + current.saleId + '?dir=' + dir);
            if (!response.ok) return;
            const result = await response.json();
            const header = result.sale || result.Sale;
            const id = header && (header.id_Sale || header.iD_Sale || header.ID_Sale);
            if (id) await openCompletedSale(id);
        } catch (e) { /* ignore empty neighbor */ }
    }

    async function searchBill(query) {
        const q = (query || '').trim();
        if (!q) return;
        try {
            const response = await fetch(API.getList, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    SearchText: q,
                    PaymentMethod: '',
                    PageIndex: 1,
                    PageSize: 5,
                    SortColumn: 0,
                    SortMode: 'DESC'
                })
            });
            const result = await response.json();
            const rows = result.tableData || result.TableData || [];
            if (!rows.length) {
                showWarning('Bills', 'No bill matched that search.');
                return;
            }
            const id = rows[0].id_Sale || rows[0].iD_Sale || rows[0].ID_Sale;
            await openCompletedSale(id);
        } catch (e) {
            showError('Bills', 'Could not search bills.');
        }
    }

    async function openCompletedSale(id) {
        const data = await fetchJson(API.getById + '/' + id);
        if (!data) return;
        const header = data.saleHeader || data.SaleHeader;
        const details = data.saleDetails || data.SaleDetails || [];
        if (!header) {
            showError('Error', 'Sale not found.');
            return;
        }
        const historyEl = document.getElementById('posHistoryOffcanvas');
        if (historyEl) bootstrap.Offcanvas.getInstance(historyEl)?.hide();
        current = {
            localId: 's-' + id,
            saleId: header.id_Sale || header.iD_Sale || header.ID_Sale,
            status: 'completed',
            invoiceLabel: header.invoiceNumber || header.InvoiceNumber || '',
            hasReturns: header.hasReturns || header.HasReturns || false,
            saleDate: toDateInput(header.saleDate || header.SaleDate),
            items: details.map(mapDetailToItem),
            customer: toCustomer(header.customerName || header.CustomerName, header.customerPhone || header.CustomerPhone),
            payment: header.paymentMethod || header.PaymentMethod || 'Cash',
            notes: header.notes || header.Notes || '',
            tendered: ''
        };
        renderAll();
    }

    function mapDetailToItem(d) {
        return {
            variantId: d.fk_ProductVariant || d.FK_ProductVariant,
            productId: d.fk_Product || d.FK_Product,
            name: d.productName || d.ProductName || '',
            variant: d.variantLabel || d.VariantLabel || '',
            sku: d.sku || d.SKU || '',
            barcode: '',
            qty: d.quantity || d.Quantity || 1,
            price: Number(d.sellingPrice || d.SellingPrice || 0),
            mrp: d.mrp == null && d.MRP == null ? null : Number(d.mrp ?? d.MRP),
            stock: 9999
        };
    }

    function toCustomer(name, phone) {
        const n = (name || '').trim();
        const p = (phone || '').trim();
        if ((!n || /^walk-in$/i.test(n)) && !p) return null;
        return { name: /^walk-in$/i.test(n) ? '' : n, phone: p, userId: null };
    }

    async function searchSkus(query) {
        const q = (query || '').trim();
        if (q.length < 1) {
            skuResults = [];
            closeSkuResults();
            return;
        }
        try {
            const response = await fetch(API.searchSkus + '?q=' + encodeURIComponent(q));
            skuResults = response.ok ? await response.json() : await fallbackSearchSkus(q);
            const exact = skuResults.find(function (s) {
                const barcode = String(s.barcode || s.Barcode || '').toLowerCase();
                const sku = String(s.sku || s.SKU || '').toLowerCase();
                return barcode === q.toLowerCase() || sku === q.toLowerCase();
            });
            if (exact && skuResults.length === 1) {
                addSku(exact);
                return;
            }
            skuIndex = 0;
            renderSkuResults();
        } catch (e) {
            skuResults = await fallbackSearchSkus(q);
            skuIndex = 0;
            if (skuResults.length) renderSkuResults();
            else closeSkuResults();
        }
    }

    async function fallbackSearchSkus(q) {
        const needle = q.toLowerCase();
        try {
            if (!window.__posProducts) {
                const response = await fetch('/Admin/Sale/GetProducts');
                window.__posProducts = response.ok ? await response.json() : [];
            }
            const matches = (window.__posProducts || []).filter(function (p) {
                return String(p.name || p.Name || '').toLowerCase().indexOf(needle) >= 0;
            }).slice(0, 8);
            const rows = [];
            for (let i = 0; i < matches.length; i++) {
                const product = matches[i];
                const pid = product.productId || product.ID_Product || product.iD_Product;
                const response = await fetch('/Admin/Sale/GetProductVariants/' + pid);
                const variants = response.ok ? await response.json() : [];
                variants.forEach(function (v) {
                    rows.push({
                        fk_Product: pid,
                        id_ProductVariant: v.id_ProductVariant || v.iD_ProductVariant || v.ID_ProductVariant,
                        productName: product.name || product.Name || '',
                        variantLabel: v.variantLabel || v.VariantLabel || '',
                        sku: v.sku || v.SKU || '',
                        barcode: v.barcode || v.Barcode || '',
                        sellingPrice: v.sellingPrice || v.SellingPrice || 0,
                        mrp: v.mrp || v.MRP,
                        availableQty: v.availableQty ?? v.AvailableQty ?? 0
                    });
                });
            }
            return rows.filter(function (row) {
                return (row.productName + ' ' + row.variantLabel + ' ' + row.sku + ' ' + row.barcode)
                    .toLowerCase().indexOf(needle) >= 0;
            }).slice(0, 20);
        } catch (e) {
            return [];
        }
    }

    function renderSkuResults() {
        const box = document.getElementById('posSkuResults');
        if (!skuResults.length) {
            box.innerHTML = '<div class="pos-empty" style="min-height:80px">No matching SKU</div>';
            box.classList.add('is-open');
            return;
        }
        box.innerHTML = skuResults.map(function (s, i) {
            const name = s.productName || s.ProductName || '';
            const label = s.variantLabel || s.VariantLabel || '';
            const sku = s.sku || s.SKU || '';
            const price = Number(s.sellingPrice || s.SellingPrice || 0);
            const stock = s.availableQty ?? s.AvailableQty ?? 0;
            return '<button type="button" class="pos-result' + (i === skuIndex ? ' is-active' : '') + '" data-index="' + i + '">' +
                '<div><div class="pos-result-name">' + escapeHtml(name) + (label ? ' · ' + escapeHtml(label) : '') + '</div>' +
                '<div class="pos-result-meta">' + escapeHtml(sku) + '</div></div>' +
                '<div class="pos-result-price">₹' + price.toFixed(2) + '</div>' +
                '<div class="pos-result-stock">' + stock + ' in stock</div></button>';
        }).join('');
        box.querySelectorAll('.pos-result').forEach(function (btn) {
            btn.addEventListener('click', function () { addSku(skuResults[Number(btn.dataset.index)]); });
        });
        box.classList.add('is-open');
    }

    function closeSkuResults() {
        document.getElementById('posSkuResults').classList.remove('is-open');
    }

    function addSku(sku) {
        if (isLocked()) {
            showWarning('Bill locked', 'Start a new bill to add products.');
            return;
        }
        const variantId = sku.id_ProductVariant || sku.iD_ProductVariant || sku.ID_ProductVariant;
        const stock = sku.availableQty ?? sku.AvailableQty ?? 0;
        const existing = current.items.find(function (i) { return Number(i.variantId) === Number(variantId); });
        if (existing) {
            if (existing.qty + 1 > stock) {
                showWarning('Stock', 'Only ' + stock + ' available for this SKU.');
                return;
            }
            existing.qty += 1;
        } else {
            if (stock <= 0) {
                showWarning('Stock', 'This SKU is out of stock.');
                return;
            }
            current.items.push({
                variantId: variantId,
                productId: sku.fk_Product || sku.FK_Product,
                name: sku.productName || sku.ProductName || '',
                variant: sku.variantLabel || sku.VariantLabel || '',
                sku: sku.sku || sku.SKU || '',
                barcode: sku.barcode || sku.Barcode || '',
                qty: 1,
                price: Number(sku.sellingPrice || sku.SellingPrice || 0),
                mrp: sku.mrp == null && sku.MRP == null ? null : Number(sku.mrp ?? sku.MRP),
                stock: stock
            });
        }
        document.getElementById('posSkuSearch').value = '';
        skuResults = [];
        closeSkuResults();
        persistDrafts();
        renderItems();
        renderSummary();
        renderChrome();
        focusProductSearch();
    }

    function changeQty(variantId, nextQty) {
        if (isLocked()) return;
        const item = current.items.find(function (i) { return Number(i.variantId) === Number(variantId); });
        if (!item) return;
        const qty = parseInt(nextQty, 10);
        if (!qty || qty < 1) {
            removeItem(variantId);
            return;
        }
        if (qty > item.stock) {
            showWarning('Stock', 'Only ' + item.stock + ' available.');
            item.qty = item.stock;
        } else {
            item.qty = qty;
        }
        persistDrafts();
        renderItems();
        renderSummary();
        renderChrome();
    }

    function removeItem(variantId) {
        if (isLocked()) return;
        current.items = current.items.filter(function (i) { return Number(i.variantId) !== Number(variantId); });
        persistDrafts();
        renderItems();
        renderSummary();
        renderChrome();
    }

    async function searchCustomers(query) {
        const q = (query || '').trim();
        const box = document.getElementById('posCustomerResults');
        if (q.length < 2) {
            box.classList.remove('is-open');
            box.innerHTML = '';
            return;
        }
        try {
            const response = await fetch(API.searchCustomers + '?q=' + encodeURIComponent(q));
            const rows = response.ok ? await response.json() : await fallbackSearchCustomers(q);
            if (!rows.length) {
                box.innerHTML = '<button type="button" class="pos-customer-empty" onclick="posBilling.openCreateCustomer()">' +
                    'No customer found. <strong>+ Create Customer</strong></button>';
                box.classList.add('is-open');
                return;
            }
            box.innerHTML = rows.map(function (c, i) {
                return '<button type="button" class="pos-customer-result" data-index="' + i + '">' +
                    '<strong>' + escapeHtml(c.name || c.Name || '') + '</strong>' +
                    '<div class="pos-result-meta">' + escapeHtml(c.phone || c.Phone || '') + '</div></button>';
            }).join('');
            box.querySelectorAll('.pos-customer-result').forEach(function (btn) {
                btn.addEventListener('click', function () { selectCustomer(rows[Number(btn.dataset.index)]); });
            });
            box.classList.add('is-open');
        } catch (e) {
            const rows = await fallbackSearchCustomers(q);
            if (!rows.length) {
                box.innerHTML = '<button type="button" class="pos-customer-empty" onclick="posBilling.openCreateCustomer()">' +
                    'No customer found. <strong>+ Create Customer</strong></button>';
                box.classList.add('is-open');
                return;
            }
            box.innerHTML = rows.map(function (c, i) {
                return '<button type="button" class="pos-customer-result" data-index="' + i + '">' +
                    '<strong>' + escapeHtml(c.name || '') + '</strong>' +
                    '<div class="pos-result-meta">' + escapeHtml(c.phone || '') + '</div></button>';
            }).join('');
            box.querySelectorAll('.pos-customer-result').forEach(function (btn) {
                btn.addEventListener('click', function () { selectCustomer(rows[Number(btn.dataset.index)]); });
            });
            box.classList.add('is-open');
        }
    }

    async function fallbackSearchCustomers(q) {
        try {
            const response = await fetch(API.getList, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    SearchText: q,
                    PaymentMethod: '',
                    PageIndex: 1,
                    PageSize: 20,
                    SortColumn: 0,
                    SortMode: 'DESC'
                })
            });
            const result = response.ok ? await response.json() : { tableData: [] };
            const rows = result.tableData || result.TableData || [];
            const seen = {};
            return rows.map(function (s) {
                return {
                    name: s.customerName || s.CustomerName || '',
                    phone: s.customerPhone || s.CustomerPhone || ''
                };
            }).filter(function (c) {
                if (!c.phone || /^walk-in$/i.test(c.name)) return false;
                const key = c.phone.toLowerCase();
                if (seen[key]) return false;
                seen[key] = true;
                return true;
            });
        } catch (e) {
            return [];
        }
    }

    function selectCustomer(row) {
        if (isLocked()) return;
        current.customer = {
            name: row.name || row.Name || '',
            phone: row.phone || row.Phone || '',
            userId: row.userId || row.UserId || null
        };
        document.getElementById('posCustomerSearch').value = '';
        closeCustomerResults();
        persistDrafts();
        renderCustomer();
    }

    function clearCustomer() {
        if (isLocked()) return;
        current.customer = null;
        persistDrafts();
        renderCustomer();
    }

    function openCreateCustomer() {
        if (isLocked()) return;
        const q = document.getElementById('posCustomerSearch').value.trim();
        document.getElementById('posNewCustomerName').value = '';
        document.getElementById('posNewCustomerPhone').value = /^\d{6,}$/.test(q) ? q : '';
        closeCustomerResults();
        new bootstrap.Modal(document.getElementById('posCreateCustomerModal')).show();
        setTimeout(function () { document.getElementById('posNewCustomerName').focus(); }, 200);
    }

    function createCustomerFromPopup() {
        const name = document.getElementById('posNewCustomerName').value.trim();
        const phone = document.getElementById('posNewCustomerPhone').value.trim();
        if (!name || !phone) {
            showWarning('Validation', 'Name and mobile number are required.');
            return;
        }
        current.customer = { name: name, phone: phone, userId: null };
        persistDrafts();
        bootstrap.Modal.getInstance(document.getElementById('posCreateCustomerModal'))?.hide();
        renderCustomer();
    }

    function closeCustomerResults() {
        document.getElementById('posCustomerResults').classList.remove('is-open');
    }

    function setPayment(method) {
        if (isLocked()) return;
        current.payment = method;
        persistDrafts();
        renderPayment();
    }

    function holdBill() {
        persistDrafts();
        showSuccess('Held', 'This bill stays pending. Use F10 for the next customer.');
    }

    async function completeBill() {
        if (saving) return;
        if (current.status === 'completed' && current.hasReturns) {
            showWarning('Locked', 'This bill has a return and cannot be changed.');
            return;
        }
        if (!current.items.length) {
            showWarning('Validation', 'Add at least one product.');
            return;
        }
        for (let i = 0; i < current.items.length; i++) {
            const line = current.items[i];
            if (current.status === 'pending' && line.qty > line.stock) {
                showWarning('Stock', line.name + ' exceeds available stock.');
                return;
            }
        }
        const payload = {
            ID_Sale: current.saleId || 0,
            SaleDate: current.saleDate || todayIso(),
            CustomerName: current.customer?.name || '',
            CustomerPhone: current.customer?.phone || '',
            PaymentMethod: current.payment || 'Cash',
            Notes: current.notes || '',
            SaleDetails: current.items.map(function (line) {
                return {
                    FK_ProductVariant: line.variantId,
                    Quantity: line.qty,
                    SellingPrice: line.price,
                    MRP: line.mrp
                };
            })
        };
        const url = current.saleId ? API.update : API.create;
        saving = true;
        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            const result = await response.json().catch(function () { return null; });
            const ok = response.ok && (result?.statusCode ?? result?.StatusCode) === true;
            if (!ok) {
                showError('Error', result?.responseMsg || result?.ResponseMsg || result?.message || 'Could not complete bill.');
                return;
            }
            drafts = drafts.filter(function (b) { return b.localId !== current.localId; });
            persistDrafts();
            showSuccess('Completed', result?.responseMsg || result?.ResponseMsg || 'Bill saved.');
            newBill();
            if (document.getElementById('posHistoryOffcanvas')?.classList.contains('show')) {
                loadHistory(historyPage);
            }
        } catch (e) {
            showError('Error', 'Could not complete bill.');
        } finally {
            saving = false;
        }
    }

    function isLocked() {
        return current.status === 'completed' && current.hasReturns;
    }

    function isReadOnly() {
        return current.status === 'completed';
    }

    function totals() {
        const itemCount = current.items.reduce(function (sum, i) { return sum + i.qty; }, 0);
        const subtotal = current.items.reduce(function (sum, i) { return sum + (i.qty * i.price); }, 0);
        return { itemCount: itemCount, subtotal: subtotal, total: subtotal };
    }

    function renderAll() {
        renderChrome();
        renderDrafts();
        renderItems();
        renderSummary();
        renderCustomer();
        renderPayment();
    }

    function renderChrome() {
        const pending = current.status === 'pending';
        const label = pending
            ? 'New Bill'
            : (current.invoiceLabel || ('Bill #' + current.saleId));
        document.getElementById('posBillTitle').textContent = label;
        const pill = document.getElementById('posBillStatus');
        pill.textContent = pending ? 'PENDING' : 'COMPLETED';
        pill.className = 'pos-pill ' + (pending ? 'pos-pill-pending' : 'pos-pill-done');
        document.getElementById('posClock').textContent = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        const completeBtn = document.getElementById('posCompleteBtn');
        completeBtn.textContent = 'Complete Bill';
        completeBtn.disabled = isReadOnly() || !current.items.length;
        completeBtn.style.display = isReadOnly() ? 'none' : '';
        document.getElementById('posHoldBtn').style.display = isReadOnly() ? 'none' : '';
        document.getElementById('posViewReturn').style.display = current.saleId ? '' : 'none';
        document.getElementById('posViewReturn').href = current.saleId ? '/Admin/SalesReturn?saleId=' + current.saleId : '#';
        const fieldsLocked = isReadOnly();
        document.getElementById('posSkuSearch').disabled = fieldsLocked;
        document.getElementById('posCustomerSearch').disabled = fieldsLocked;
        document.getElementById('posNotes').disabled = fieldsLocked;
        document.getElementById('posNotes').value = current.notes || '';
        document.getElementById('posTendered').disabled = fieldsLocked;
        document.getElementById('posTendered').value = current.tendered || '';
    }

    function renderDrafts() {
        const box = document.getElementById('posDrafts');
        const pending = drafts.filter(function (b) { return b.status === 'pending'; });
        box.innerHTML = pending.map(function (b, i) {
            const active = current && current.localId === b.localId;
            const count = (b.items || []).length;
            return '<button type="button" class="pos-draft-chip' + (active ? ' is-active' : '') + '" data-id="' + escapeHtml(b.localId) + '">Draft ' + (i + 1) + (count ? ' · ' + count : '') + '</button>';
        }).join('');
        box.querySelectorAll('.pos-draft-chip').forEach(function (btn) {
            btn.addEventListener('click', function () { selectDraft(btn.dataset.id); });
        });
    }

    function renderItems() {
        const body = document.getElementById('posItemsBody');
        if (!current.items.length) {
            body.innerHTML = '<div class="pos-empty"><i class="ti ti-barcode fs-3"></i><div>Search a name, SKU or barcode to add the first item.</div></div>';
            return;
        }
        const locked = isReadOnly();
        body.innerHTML = current.items.map(function (item) {
            const low = item.qty > item.stock;
            const title = escapeHtml(item.name) + (item.variant ? ' · ' + escapeHtml(item.variant) : '');
            return '<div class="pos-item" data-variant="' + item.variantId + '">' +
                '<div class="pos-item-info"><div class="pos-item-name">' + title + '</div>' +
                '<div class="pos-item-meta' + (low ? ' is-low' : '') + '">' + escapeHtml(item.sku) + ' · ' + item.stock + ' left</div></div>' +
                '<div class="pos-qty">' +
                '<button type="button" data-act="dec" ' + (locked ? 'disabled' : '') + '>−</button>' +
                '<input type="number" min="1" value="' + item.qty + '" ' + (locked ? 'disabled' : '') + '>' +
                '<button type="button" data-act="inc" ' + (locked ? 'disabled' : '') + '>+</button></div>' +
                '<div class="pos-item-price">₹' + Number(item.price).toFixed(2) + '</div>' +
                '<div class="pos-item-total">₹' + (item.qty * item.price).toFixed(2) + '</div>' +
                '<button type="button" class="pos-item-remove" data-act="remove" ' + (locked ? 'disabled' : '') + '><i class="ti ti-x"></i></button></div>';
        }).join('');
        body.querySelectorAll('.pos-item').forEach(function (row) {
            const id = row.dataset.variant;
            const item = current.items.find(function (i) { return String(i.variantId) === String(id); });
            row.querySelector('[data-act="dec"]')?.addEventListener('click', function () { changeQty(id, item.qty - 1); });
            row.querySelector('[data-act="inc"]')?.addEventListener('click', function () { changeQty(id, item.qty + 1); });
            row.querySelector('[data-act="remove"]')?.addEventListener('click', function () { removeItem(id); });
            row.querySelector('input')?.addEventListener('change', function (e) { changeQty(id, e.target.value); });
        });
    }

    function renderSummary() {
        const t = totals();
        document.getElementById('posItemCount').textContent = t.itemCount + (t.itemCount === 1 ? ' item' : ' items');
        document.getElementById('posSubtotal').textContent = '₹' + t.subtotal.toFixed(2);
        document.getElementById('posTotal').textContent = '₹' + t.total.toFixed(2);
        renderPaymentExtras();
    }

    function renderCustomer() {
        const selected = document.getElementById('posCustomerSelected');
        if (!current.customer) {
            selected.innerHTML = '<div><strong>None</strong><span class="pos-result-meta">Customer is optional</span></div>';
            return;
        }
        selected.innerHTML = '<div><strong>' + escapeHtml(current.customer.name || 'Customer') + '</strong>' +
            '<span class="pos-result-meta">' + escapeHtml(current.customer.phone || '') + '</span></div>' +
            (isReadOnly() ? '' : '<button type="button" class="btn btn-sm btn-outline-secondary" id="posClearCustomer">Clear</button>');
        document.getElementById('posClearCustomer')?.addEventListener('click', clearCustomer);
    }

    function renderPayment() {
        document.querySelectorAll('.pos-pay-btn').forEach(function (btn) {
            btn.classList.toggle('is-active', btn.dataset.pay === current.payment);
            btn.disabled = isReadOnly();
        });
        renderPaymentExtras();
    }

    function renderPaymentExtras() {
        current.tendered = document.getElementById('posTendered').value;
        const cashBox = document.getElementById('posCashExtras');
        cashBox.style.display = current.payment === 'Cash' ? 'block' : 'none';
        const tendered = parseFloat(current.tendered);
        const change = !isNaN(tendered) ? tendered - totals().total : 0;
        document.getElementById('posChange').textContent = '₹' + Math.max(0, change).toFixed(2);
    }

    async function loadHistory(page) {
        historyPage = page || 1;
        const listing = window.emptyState;
        listing.showListingLoader('posHistoryBody', 6);
        const input = {
            SearchText: document.getElementById('posHistorySearch').value || '',
            PaymentMethod: document.getElementById('posHistoryPayment').value || '',
            FromDate: document.getElementById('posHistoryFrom').value || null,
            ToDate: document.getElementById('posHistoryTo').value || null,
            PageIndex: historyPage,
            PageSize: historyPageSize,
            SortColumn: 0,
            SortMode: 'DESC'
        };
        try {
            const response = await fetch(API.getList, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(input)
            });
            const result = await response.json();
            if (!response.ok || result?.success === false) {
                listing.handleListingError('Sale List Load', { url: API.getList, method: 'POST', payload: input, response, result }, 'posHistoryBody', 6);
                return;
            }
            const rows = result.tableData || result.TableData || [];
            const settings = result.tableSettings || result.TableSettings;
            if (!rows.length) listing.showListingEmpty('posHistoryBody', 6);
            else document.getElementById('posHistoryBody').innerHTML = rows.map(historyRow).join('');
            if (historyPager && settings) historyPager.render(settings);
        } catch (error) {
            listing.handleListingError('Sale List Load', { url: API.getList, method: 'POST', payload: input, error }, 'posHistoryBody', 6);
        } finally {
            listing.endListingLoad('posHistoryBody');
        }
    }

    function historyRow(s) {
        const id = s.id_Sale || s.iD_Sale || s.ID_Sale || 0;
        const inv = s.invoiceNumber || s.InvoiceNumber || '-';
        const name = s.customerName || s.CustomerName || 'None';
        const displayName = /^walk-in$/i.test(name) ? 'None' : name;
        const phone = s.customerPhone || s.CustomerPhone || '';
        const date = s.saleDate || s.SaleDate;
        const pay = s.paymentMethod || s.PaymentMethod || 'Cash';
        const total = s.totalAmount || s.TotalAmount || 0;
        const hasReturns = s.hasReturns || s.HasReturns || false;
        return '<tr>' +
            '<td><a href="javascript:void(0)" onclick="posBilling.openCompletedSale(' + id + ')">' + escapeHtml(inv) + '</a></td>' +
            '<td>' + escapeHtml(displayName) + (phone ? '<div class="text-muted small">' + escapeHtml(phone) + '</div>' : '') + '</td>' +
            '<td>' + (date ? new Date(date).toLocaleDateString() : '-') + '</td>' +
            '<td>' + escapeHtml(pay) + '</td>' +
            '<td><strong>₹' + Number(total).toFixed(2) + '</strong></td>' +
            '<td class="text-end">' +
            '<button class="btn-action btn-view" onclick="posBilling.openCompletedSale(' + id + ')" title="Open"><i class="ti ti-external-link"></i></button>' +
            (hasReturns ? '' : '<button class="btn-action btn-delete" data-action="Delete" onclick="posBilling.openDelete(' + id + ')" title="Delete"><i class="ti ti-trash"></i></button>') +
            '<a class="btn-action btn-view" href="/Admin/SalesReturn?saleId=' + id + '" title="Return"><i class="ti ti-receipt-refund"></i></a>' +
            '</td></tr>';
    }

    function openDelete(id) {
        document.getElementById('deleteSaleID').value = id;
        document.getElementById('deleteReason').value = '';
        new bootstrap.Modal(document.getElementById('deleteModal')).show();
    }

    async function confirmDelete() {
        const input = {
            ID_Sale: parseInt(document.getElementById('deleteSaleID').value, 10),
            CancelledReason: document.getElementById('deleteReason').value.trim()
        };
        try {
            const response = await fetch(API.delete, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(input)
            });
            const result = await response.json().catch(function () { return null; });
            const ok = response.ok && (result?.statusCode ?? result?.StatusCode) === true;
            if (ok) {
                bootstrap.Modal.getInstance(document.getElementById('deleteModal'))?.hide();
                loadHistory(historyPage);
                if (current.saleId === input.ID_Sale) newBill();
                showSuccess('Deleted', result?.responseMsg || result?.ResponseMsg || 'Sale deleted.');
            } else {
                showError('Error', result?.responseMsg || result?.ResponseMsg || 'Could not delete sale.');
            }
        } catch (e) {
            showError('Error', 'Could not delete sale.');
        }
    }

    async function fetchJson(url) {
        try {
            const response = await fetch(url);
            if (!response.ok) {
                showError('Error', 'Failed to load details.');
                return null;
            }
            return await response.json();
        } catch (e) {
            showError('Error', 'Failed to load details.');
            return null;
        }
    }

    function focusProductSearch() {
        document.getElementById('posSkuSearch')?.focus();
    }

    function toDateInput(value) {
        if (!value) return todayIso();
        const d = new Date(value);
        if (isNaN(d.getTime())) return todayIso();
        return d.toISOString().slice(0, 10);
    }

    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    window.posBilling = {
        newBill: newBill,
        navigateBill: navigateBill,
        completeBill: completeBill,
        holdBill: holdBill,
        setPayment: setPayment,
        openCreateCustomer: openCreateCustomer,
        createCustomerFromPopup: createCustomerFromPopup,
        openCompletedSale: openCompletedSale,
        loadHistory: loadHistory,
        openDelete: openDelete,
        confirmDelete: confirmDelete,
        resetHistoryFilters: function () {
            document.getElementById('posHistorySearch').value = '';
            document.getElementById('posHistoryPayment').value = '';
            document.getElementById('posHistoryFrom').value = '';
            document.getElementById('posHistoryTo').value = '';
            loadHistory(1);
        }
    };
})();
