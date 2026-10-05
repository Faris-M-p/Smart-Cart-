(function () {
    function escapeHtml(value) {
        return String(value || "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    function money(value) {
        return "₹" + Number(value || 0).toLocaleString("en-IN", { maximumFractionDigits: 2 });
    }

    function detailsUrl(item) {
        var key = item.slug || item.Slug || item.productId || item.ProductId;
        return key ? "/Shop/Details/" + encodeURIComponent(key) : "/Shop";
    }

    function renderItems(container, items) {
        container.innerHTML = (items || []).map(function (item) {
            var image = item.imageUrl || item.ImageUrl;
            var name = escapeHtml(item.name || item.Name);
            var href = detailsUrl(item);
            return '<article class="shop-checkout-line">' +
                (image
                    ? '<img src="' + escapeHtml(image) + '" alt="' + name + '">'
                    : '<div class="shop-checkout-fallback">No image</div>') +
                "<div>" +
                    "<h4><a href=\"" + escapeHtml(href) + "\">" + name + "</a></h4>" +
                    "<p>" + escapeHtml(item.label || item.Label || item.sku || item.SKU) +
                        " · Qty " + (item.quantity || item.Quantity) + "</p>" +
                "</div>" +
                "<strong>" + money(item.lineTotal ?? item.LineTotal ?? ((item.price || item.Price) * (item.quantity || item.Quantity))) + "</strong>" +
            "</article>";
        }).join("");
    }

    function flash(el, message, persist) {
        if (window.smartCartFlash) {
            window.smartCartFlash(el, { message: message, persist: !!persist });
            return;
        }
        if (!el) return;
        el.textContent = message || "";
        el.hidden = !message;
    }

    var savedAddressesList = [];
    var customerProfile = {};
    var tempFetchedLocation = null;
    var currentActiveType = "Home";
    var ALLOWED_ADDRESS_TYPES = ["Home", "Work", "Office", "Other"];

    // =========================================================================
    // MODAL OPEN / CLOSE (FAIL-SAFE STANDALONE WITH BOOTSTRAP SUPPORT)
    // =========================================================================
    function openModal(target) {
        var modalEl = typeof target === "string" ? document.getElementById(target) : target;
        if (!modalEl) return;

        if (window.bootstrap && window.bootstrap.Modal) {
            try {
                var inst = window.bootstrap.Modal.getInstance(modalEl) || new window.bootstrap.Modal(modalEl);
                inst.show();
                return;
            } catch (e) {
                console.warn("Bootstrap modal exception, falling back to standalone", e);
            }
        }

        modalEl.classList.add("shop-modal-open");
        modalEl.classList.add("show");
        modalEl.removeAttribute("aria-hidden");
        modalEl.setAttribute("aria-modal", "true");
        document.body.classList.add("modal-open");
    }

    function closeModal(target) {
        var modalEl = typeof target === "string" ? document.getElementById(target) : target;
        if (!modalEl) return;

        if (window.bootstrap && window.bootstrap.Modal) {
            try {
                var inst = window.bootstrap.Modal.getInstance(modalEl);
                if (inst) inst.hide();
            } catch (e) { }
        }

        modalEl.classList.remove("shop-modal-open");
        modalEl.classList.remove("show");
        modalEl.setAttribute("aria-hidden", "true");
        modalEl.removeAttribute("aria-modal");

        if (!document.querySelector(".modal.show, .shop-modal-open")) {
            document.body.classList.remove("modal-open");
        }
    }

    document.addEventListener("click", function (e) {
        var dismissBtn = e.target.closest('[data-bs-dismiss="modal"], .btn-close');
        if (dismissBtn) {
            var parentModal = dismissBtn.closest(".modal");
            if (parentModal) closeModal(parentModal);
            return;
        }

        if (e.target.classList.contains("modal") && (e.target.classList.contains("show") || e.target.classList.contains("shop-modal-open"))) {
            closeModal(e.target);
        }
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            var openModalEl = document.querySelector(".modal.show, .shop-modal-open");
            if (openModalEl) closeModal(openModalEl);
        }
    });

    // =========================================================================
    // ADDRESS FIELD POPULATION & SYNCHRONIZATION
    // =========================================================================
    function populateAddressFields(addr) {
        if (!addr) return;

        var id = addr.addressId || addr.AddressId || 0;
        var type = addr.addressType || addr.AddressType || "Home";
        var name = addr.receiverName || addr.ReceiverName || "";
        var phone = addr.phone || addr.Phone || "";
        var line = addr.addressLine || addr.AddressLine || "";
        var city = addr.city || addr.City || "";
        var pin = addr.pincode || addr.Pincode || "";
        var lat = addr.latitude || addr.Latitude || "";
        var lon = addr.longitude || addr.Longitude || "";

        currentActiveType = type;

        var elId = document.getElementById("checkout-selected-address-id");
        var elType = document.getElementById("checkout-selected-address-type");
        var elName = document.getElementById("checkout-selected-name");
        var elPhone = document.getElementById("checkout-selected-phone");
        var elLine = document.getElementById("checkout-selected-address-line");
        var elCity = document.getElementById("checkout-selected-city");
        var elPin = document.getElementById("checkout-selected-pincode");
        var elLat = document.getElementById("checkout-selected-latitude");
        var elLon = document.getElementById("checkout-selected-longitude");

        if (elId) elId.value = id;
        if (elType) elType.value = type;
        if (elName) elName.value = name;
        if (elPhone) elPhone.value = phone;
        if (elLine) elLine.value = line;
        if (elCity) elCity.value = city;
        if (elPin) elPin.value = pin;
        if (elLat) elLat.value = lat;
        if (elLon) elLon.value = lon;

        updateAddressPillsUI();
        updateDeliveryPreview();
    }

    function updateAddressPillsUI() {
        var badge = document.getElementById("checkout-active-tag-badge");
        if (badge) {
            badge.textContent = currentActiveType;
        }

        document.querySelectorAll("#checkout-address-type-selector .shop-type-pill").forEach(function (pill) {
            var type = pill.getAttribute("data-type") || "Home";
            var isActive = type.toLowerCase() === currentActiveType.toLowerCase();
            pill.classList.toggle("active", isActive);

            // Check if saved address exists for this type
            var hasSaved = savedAddressesList.some(function (a) {
                return (a.addressType || a.AddressType || "").toLowerCase() === type.toLowerCase();
            });

            var span = pill.querySelector("span");
            if (span) {
                span.textContent = type + (hasSaved ? " (Saved)" : "");
            }
        });
    }

    function selectAddressByType(type) {
        currentActiveType = type;
        var elType = document.getElementById("checkout-selected-address-type");
        if (elType) elType.value = type;

        var existing = savedAddressesList.find(function (a) {
            return (a.addressType || a.AddressType || "").toLowerCase() === type.toLowerCase();
        });

        if (existing) {
            populateAddressFields(existing);
        } else {
            // Address not saved yet for this type: keep existing name/phone or use profile, clear location
            var elId = document.getElementById("checkout-selected-address-id");
            if (elId) elId.value = "0";

            var elName = document.getElementById("checkout-selected-name");
            var elPhone = document.getElementById("checkout-selected-phone");
            if (elName && !elName.value) elName.value = customerProfile.fullName || customerProfile.FullName || "";
            if (elPhone && !elPhone.value) elPhone.value = customerProfile.phone || customerProfile.Phone || "";

            var elLine = document.getElementById("checkout-selected-address-line");
            var elCity = document.getElementById("checkout-selected-city");
            var elPin = document.getElementById("checkout-selected-pincode");
            var elLat = document.getElementById("checkout-selected-latitude");
            var elLon = document.getElementById("checkout-selected-longitude");

            if (elLine) elLine.value = "";
            if (elCity) elCity.value = "";
            if (elPin) elPin.value = "";
            if (elLat) elLat.value = "";
            if (elLon) elLon.value = "";

            updateAddressPillsUI();
            updateDeliveryPreview();
        }
    }

    function updateDeliveryPreview() {
        var delPreview = document.getElementById("checkout-delivery-preview");
        var previewName = document.getElementById("preview-deliver-name");
        var previewAddr = document.getElementById("preview-deliver-address");
        if (!delPreview || !previewName || !previewAddr) return;

        var name = (document.getElementById("checkout-selected-name")?.value || "").trim();
        var type = document.getElementById("checkout-selected-address-type")?.value || "Home";
        var line = (document.getElementById("checkout-selected-address-line")?.value || "").trim();
        var city = (document.getElementById("checkout-selected-city")?.value || "").trim();
        var pin = (document.getElementById("checkout-selected-pincode")?.value || "").trim();

        if (name || line) {
            previewName.textContent = name ? (name + " (" + type + ")") : type;
            previewAddr.textContent = (line ? line + ", " : "") + city + (pin ? " - " + pin : "");
            delPreview.hidden = false;
        } else {
            delPreview.hidden = true;
        }
    }

    // Bind real-time input typing to preview
    ["checkout-selected-name", "checkout-selected-phone", "checkout-selected-address-line", "checkout-selected-city", "checkout-selected-pincode"].forEach(function (id) {
        var el = document.getElementById(id);
        if (el) {
            el.addEventListener("input", updateDeliveryPreview);
        }
    });

    // =========================================================================
    // SAVED ADDRESSES DRAWER / MODAL
    // =========================================================================
    function renderDrawerSavedAddresses() {
        var drawerListEl = document.getElementById("drawer-addresses-list");
        if (!drawerListEl) return;

        if (!savedAddressesList || !savedAddressesList.length) {
            drawerListEl.innerHTML = '<div class="text-center py-4 text-muted">' +
                '<p class="mb-1">No saved addresses found.</p>' +
                '<small>You can fill out the form fields directly on checkout.</small>' +
            '</div>';
            return;
        }

        var currentSelectedId = Number(document.getElementById("checkout-selected-address-id")?.value || 0);

        drawerListEl.innerHTML = savedAddressesList.map(function (addr) {
            var id = addr.addressId || addr.AddressId;
            var type = escapeHtml(addr.addressType || addr.AddressType || "Home");
            var typeClass = "type-" + type.toLowerCase();
            var name = escapeHtml(addr.receiverName || addr.ReceiverName || "");
            var phone = escapeHtml(addr.phone || addr.Phone || "");
            var line = escapeHtml(addr.addressLine || addr.AddressLine || "");
            var city = escapeHtml(addr.city || addr.City || "");
            var pin = escapeHtml(addr.pincode || addr.Pincode || "");
            var isDefault = !!(addr.isDefault || addr.IsDefault);
            var isSel = (id === currentSelectedId);

            return '<div class="shop-drawer-card ' + (isSel ? 'is-selected' : '') + '" data-address-id="' + id + '">' +
                '<div class="shop-drawer-radio"></div>' +
                '<div class="flex-1 min-w-0 pr-2">' +
                    '<div class="d-flex align-items-center gap-2 mb-1">' +
                        '<span class="shop-address-badge ' + typeClass + '">' + type + '</span>' +
                        (isDefault ? '<span class="shop-badge-default">Default</span>' : '') +
                        (isSel ? '<span class="text-xs text-danger font-semibold ms-auto">Active</span>' : '') +
                    '</div>' +
                    '<div class="fw-bold text-dark text-sm">' + name + ' <span class="text-muted fw-normal ms-1">' + phone + '</span></div>' +
                    '<div class="text-muted text-xs mt-1 leading-snug">' + line + ', ' + city + ' - ' + pin + '</div>' +
                '</div>' +
                '<div class="d-flex align-items-center gap-2 flex-shrink-0">' +
                    '<button type="button" class="btn btn-sm btn-danger text-xs px-2.5 py-1" data-action="deliver" data-id="' + id + '">Deliver Here</button>' +
                    '<button type="button" class="shop-btn-card-delete" data-action="delete" data-id="' + id + '" title="Delete Address">&times;</button>' +
                '</div>' +
            '</div>';
        }).join("");

        drawerListEl.querySelectorAll(".shop-drawer-card").forEach(function (card) {
            card.addEventListener("click", function (e) {
                var btnDeliver = e.target.closest('[data-action="deliver"]');
                var btnDelete = e.target.closest('[data-action="delete"]');
                var id = Number(card.getAttribute("data-address-id"));

                if (btnDelete) {
                    e.stopPropagation();
                    if (confirm("Delete this saved address?")) {
                        deleteSavedAddress(id);
                    }
                    return;
                }

                // Deliver here
                var targetAddr = savedAddressesList.find(function (a) { return (a.addressId || a.AddressId) === id; });
                if (targetAddr) {
                    populateAddressFields(targetAddr);
                }
                closeModal("selectAddressDrawer");
            });
        });
    }

    async function deleteSavedAddress(addressId) {
        try {
            var res = await fetch("/Checkout/DeleteAddress", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(addressId)
            });
            var data = await res.json();
            if (data.statusCode || data.StatusCode) {
                await reloadUserAddresses();
            } else {
                alert(data.responseMsg || "Could not delete address.");
            }
        } catch (e) {
            console.error("Delete address failed", e);
        }
    }

    async function reloadUserAddresses(selectId) {
        try {
            var res = await fetch("/Checkout/GetAddresses");
            if (res.ok) {
                var list = await res.json();
                savedAddressesList = list || [];
                renderDrawerSavedAddresses();
                updateAddressPillsUI();

                if (selectId) {
                    var target = savedAddressesList.find(function (a) { return (a.addressId || a.AddressId) === selectId; });
                    if (target) populateAddressFields(target);
                }
            }
        } catch (e) {
            console.error("Reload addresses failed", e);
        }
    }

    // =========================================================================
    // LOCATION MODAL (LEAFLET OPENSTREETMAP)
    // =========================================================================
    var leafletMap = null;
    var leafletMarker = null;

    window.openLocationFetchModal = function () {
        var modalEl = document.getElementById("locationFetchModal");
        if (!modalEl) return;

        openModal(modalEl);

        setTimeout(function () {
            initLeafletOSMMap();
        }, 150);

        loadIndiaStatesForLocation();
    };

    function initLeafletOSMMap() {
        var mapContainer = document.getElementById("osm-map");
        if (!mapContainer || typeof L === "undefined") return;

        var defaultLat = 11.2588;
        var defaultLon = 75.7804;

        if (!leafletMap) {
            leafletMap = L.map("osm-map").setView([defaultLat, defaultLon], 14);

            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                attribution: '&copy; OpenStreetMap contributors',
                maxZoom: 19
            }).addTo(leafletMap);

            var redIcon = L.divIcon({
                className: "custom-leaflet-pin",
                html: '<div style="background:#ff4545;width:24px;height:24px;border-radius:50%;border:3px solid #fff;box-shadow:0 3px 10px rgba(0,0,0,0.3);position:relative;"><div style="width:8px;height:8px;background:#fff;border-radius:50%;position:absolute;top:5px;left:5px;"></div></div>',
                iconSize: [24, 24],
                iconAnchor: [12, 12]
            });

            leafletMarker = L.marker([defaultLat, defaultLon], {
                draggable: true,
                icon: redIcon
            }).addTo(leafletMap);

            leafletMarker.on("dragend", function (e) {
                var coord = e.target.getLatLng();
                updateLocationFromCoords(coord.lat, coord.lng);
            });

            leafletMap.on("click", function (e) {
                var coord = e.latlng;
                leafletMarker.setLatLng(coord);
                updateLocationFromCoords(coord.lat, coord.lng);
            });
        }

        leafletMap.invalidateSize();
        startGeolocationDetection();
    }

    function updateLocationFromCoords(lat, lon) {
        var statusText = document.getElementById("loc-status-text");
        var previewAddr = document.getElementById("loc-preview-address");
        var previewCoords = document.getElementById("loc-preview-coords");
        var btnApply = document.getElementById("btn-apply-fetched-location");

        if (statusText) statusText.innerHTML = '<span class="shop-dot-live">●</span> Resolving address...';
        if (btnApply) btnApply.disabled = true;

        tempFetchedLocation = { latitude: lat, longitude: lon, city: "", pincode: "", road: "" };

        fetch("https://nominatim.openstreetmap.org/reverse?format=json&lat=" + lat + "&lon=" + lon, {
            headers: { "Accept-Language": "en" }
        })
        .then(function (res) { return res.json(); })
        .then(function (data) {
            var addr = data.address || {};
            var city = addr.city || addr.town || addr.village || addr.suburb || addr.county || addr.state_district || "";
            var pincode = addr.postcode || "";
            var road = addr.road || addr.suburb || addr.neighbourhood || addr.residential || "";

            tempFetchedLocation.city = city;
            tempFetchedLocation.pincode = pincode;
            tempFetchedLocation.road = road;

            if (previewAddr) {
                previewAddr.textContent = (road ? road + ", " : "") + city + (pincode ? " - " + pincode : "");
            }
            if (previewCoords) {
                previewCoords.textContent = "Lat: " + lat.toFixed(5) + ", Long: " + lon.toFixed(5) + " · OpenStreetMap Real-time";
            }
            if (statusText) {
                statusText.innerHTML = '<span class="shop-dot-live">●</span> GPS Location Captured';
            }
            if (btnApply) btnApply.disabled = false;
        })
        .catch(function () {
            if (previewAddr) previewAddr.textContent = "Location pin set on map";
            if (previewCoords) previewCoords.textContent = "Lat: " + lat.toFixed(5) + ", Long: " + lon.toFixed(5);
            if (statusText) statusText.innerHTML = '<span class="shop-dot-live">●</span> Coordinates Captured';
            if (btnApply) btnApply.disabled = false;
        });
    }

    function startGeolocationDetection() {
        if (!navigator.geolocation) return;

        navigator.geolocation.getCurrentPosition(
            function (position) {
                var lat = position.coords.latitude;
                var lon = position.coords.longitude;
                if (leafletMap && leafletMarker) {
                    leafletMap.setView([lat, lon], 16);
                    leafletMarker.setLatLng([lat, lon]);
                }
                updateLocationFromCoords(lat, lon);
            },
            function (err) {
                var statusText = document.getElementById("loc-status-text");
                if (statusText) {
                    statusText.innerHTML = '<span class="text-warning">●</span> GPS Permission Denied. Drag map pin to adjust.';
                }
            },
            { enableHighAccuracy: true, timeout: 12000, maximumAge: 60000 }
        );
    }

    function initLocationModalEvents() {
        var btnApply = document.getElementById("btn-apply-fetched-location");
        if (btnApply) {
            btnApply.addEventListener("click", function () {
                if (!tempFetchedLocation) return;

                if (tempFetchedLocation.city) {
                    var elCity = document.getElementById("checkout-selected-city");
                    if (elCity) elCity.value = tempFetchedLocation.city;
                }
                if (tempFetchedLocation.pincode) {
                    var elPin = document.getElementById("checkout-selected-pincode");
                    if (elPin) elPin.value = tempFetchedLocation.pincode;
                }
                if (tempFetchedLocation.road) {
                    var elLine = document.getElementById("checkout-selected-address-line");
                    if (elLine && !elLine.value) elLine.value = tempFetchedLocation.road;
                }
                if (tempFetchedLocation.latitude) {
                    var elLat = document.getElementById("checkout-selected-latitude");
                    if (elLat) elLat.value = tempFetchedLocation.latitude;
                }
                if (tempFetchedLocation.longitude) {
                    var elLon = document.getElementById("checkout-selected-longitude");
                    if (elLon) elLon.value = tempFetchedLocation.longitude;
                }

                updateDeliveryPreview();
                closeModal("locationFetchModal");
            });
        }

        var btnSearch = document.getElementById("btn-loc-search");
        var inputSearch = document.getElementById("loc-search-input");

        function performLocationSearch() {
            var query = (inputSearch ? inputSearch.value : "").trim();
            if (!query) return;

            fetch("https://nominatim.openstreetmap.org/search?format=json&q=" + encodeURIComponent(query), {
                headers: { "Accept-Language": "en" }
            })
            .then(function (res) { return res.json(); })
            .then(function (results) {
                if (results && results.length > 0) {
                    var first = results[0];
                    var lat = parseFloat(first.lat);
                    var lon = parseFloat(first.lon);
                    if (leafletMap && leafletMarker) {
                        leafletMap.setView([lat, lon], 16);
                        leafletMarker.setLatLng([lat, lon]);
                    }
                    updateLocationFromCoords(lat, lon);
                } else {
                    alert("No matching location found.");
                }
            })
            .catch(function (e) {
                console.error("Search failed", e);
            });
        }

        if (btnSearch) btnSearch.addEventListener("click", performLocationSearch);
        if (inputSearch) {
            inputSearch.addEventListener("keydown", function (e) {
                if (e.key === "Enter") {
                    e.preventDefault();
                    performLocationSearch();
                }
            });
        }

        var btnRecenter = document.getElementById("btn-recenter-gps");
        if (btnRecenter) {
            btnRecenter.addEventListener("click", function () {
                startGeolocationDetection();
            });
        }

        var stateSel = document.getElementById("loc-state-select");
        if (stateSel) {
            stateSel.addEventListener("change", async function () {
                var iso = stateSel.value;
                await loadIndiaCitiesForState(iso);
            });
        }

        var citySel = document.getElementById("loc-city-select");
        if (citySel) {
            citySel.addEventListener("change", function () {
                var cityVal = citySel.value;
                if (cityVal) {
                    tempFetchedLocation = tempFetchedLocation || {};
                    tempFetchedLocation.city = cityVal;
                    document.getElementById("btn-apply-fetched-location").disabled = false;
                    document.getElementById("loc-preview-address").textContent = cityVal + " (Selected)";
                }
            });
        }
    }

    async function loadIndiaStatesForLocation() {
        var stateSel = document.getElementById("loc-state-select");
        if (!stateSel || stateSel.options.length > 1) return;
        try {
            var res = await fetch("/Checkout/Location/IndiaStates");
            var data = await res.json();
            if (Array.isArray(data)) {
                stateSel.innerHTML = '<option value="">Select State</option>' + data.map(function (st) {
                    var iso = st.iso2 || st.iso_2 || '';
                    var name = st.name || iso;
                    return '<option value="' + escapeHtml(iso) + '">' + escapeHtml(name) + '</option>';
                }).join("");
            }
        } catch (e) { console.error(e); }
    }

    async function loadIndiaCitiesForState(iso) {
        var citySel = document.getElementById("loc-city-select");
        if (!citySel) return;
        citySel.innerHTML = '<option value="">Select City</option>';
        if (!iso) return;
        try {
            var res = await fetch("/Checkout/Location/IndiaCities/" + encodeURIComponent(iso));
            var data = await res.json();
            if (Array.isArray(data)) {
                var names = Array.from(new Set(data.map(function (c) { return c.name; }).filter(Boolean))).sort();
                citySel.innerHTML = '<option value="">Select City</option>' + names.map(function (n) {
                    return '<option value="' + escapeHtml(n) + '">' + escapeHtml(n) + '</option>';
                }).join("");
            }
        } catch (e) { console.error(e); }
    }

    // =========================================================================
    // MAIN CHECKOUT INITIALIZATION & ORDER PLACEMENT
    // =========================================================================
    async function initCheckout() {
        var form = document.getElementById("checkout-form");
        var content = document.getElementById("checkout-content");
        var empty = document.getElementById("checkout-empty");
        var status = document.getElementById("checkout-status");
        var itemsEl = document.getElementById("checkout-items");
        if (!form || !content) return;

        var params = new URLSearchParams(window.location.search);
        var buyNow = params.get("buyNow") === "1";
        var variantId = Number(params.get("variantId") || 0);
        var quantity = Number(params.get("qty") || 1);
        if (quantity < 1) quantity = 1;

        var query = buyNow && variantId
            ? "?productVariantId=" + encodeURIComponent(variantId) + "&quantity=" + encodeURIComponent(quantity)
            : "";

        try {
            var response = await fetch("/Checkout/Preview" + query);
            if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) return;
            if (!response.ok) throw new Error("preview");

            var page = await response.json();
            var items = page.items || page.Items || [];
            var summary = page.summary || page.Summary || {};
            var customer = page.customer || page.Customer || {};
            var addresses = page.addresses || page.Addresses || [];
            var canPlace = !!(summary.canPlace || summary.CanPlace);

            customerProfile = customer;
            savedAddressesList = addresses;

            if (!items.length) {
                content.hidden = true;
                if (empty) empty.hidden = false;
                return;
            }

            if (empty) empty.hidden = true;
            content.hidden = false;
            document.getElementById("checkout-source").textContent = buyNow
                ? "Buying this item now. Your cart stays as it is."
                : "Review your cart items, select a delivery address, and place your order.";

            document.getElementById("checkout-total").textContent = money(summary.subtotal || summary.Subtotal);
            renderItems(itemsEl, items);

            // Populate Address Fields: Default Address or First Saved Address or Customer Profile
            if (savedAddressesList.length > 0) {
                var defaultAddr = savedAddressesList.find(function (a) { return a.isDefault || a.IsDefault; }) || savedAddressesList[0];
                populateAddressFields(defaultAddr);
            } else {
                // Pre-fill profile name and phone if available
                var elName = document.getElementById("checkout-selected-name");
                var elPhone = document.getElementById("checkout-selected-phone");
                if (elName) elName.value = customerProfile.fullName || customerProfile.FullName || "";
                if (elPhone) elPhone.value = customerProfile.phone || customerProfile.Phone || "";
                updateAddressPillsUI();
                updateDeliveryPreview();
            }

            renderDrawerSavedAddresses();

            // Address Pill Click Handlers (Home, Work, Office, Other)
            document.querySelectorAll("#checkout-address-type-selector .shop-type-pill").forEach(function (pill) {
                pill.addEventListener("click", function () {
                    var type = pill.getAttribute("data-type") || "Home";
                    selectAddressByType(type);
                });
            });

            // "+ New Address" Pill Handler
            var btnNewPill = document.getElementById("btn-add-new-pill");
            if (btnNewPill) {
                btnNewPill.addEventListener("click", function () {
                    // Pick the next available tag not yet saved
                    var existingTypes = savedAddressesList.map(function (a) { return (a.addressType || a.AddressType || "").toLowerCase(); });
                    var nextType = ALLOWED_ADDRESS_TYPES.find(function (t) { return !existingTypes.includes(t.toLowerCase()); }) || "Other";
                    selectAddressByType(nextType);
                    document.getElementById("checkout-selected-address-line")?.focus();
                });
            }

            // Saved Addresses Drawer Trigger
            var btnTriggerDrawer = document.getElementById("btn-trigger-address-drawer");
            if (btnTriggerDrawer) {
                btnTriggerDrawer.addEventListener("click", function () {
                    renderDrawerSavedAddresses();
                    openModal("selectAddressDrawer");
                });
            }

            var btnDrawerAddNew = document.getElementById("btn-drawer-add-new");
            if (btnDrawerAddNew) {
                btnDrawerAddNew.addEventListener("click", function () {
                    closeModal("selectAddressDrawer");
                    var existingTypes = savedAddressesList.map(function (a) { return (a.addressType || a.AddressType || "").toLowerCase(); });
                    var nextType = ALLOWED_ADDRESS_TYPES.find(function (t) { return !existingTypes.includes(t.toLowerCase()); }) || "Other";
                    selectAddressByType(nextType);
                    document.getElementById("checkout-selected-address-line")?.focus();
                });
            }

            // GPS / Map Location Button
            var btnInlineLoc = document.getElementById("btn-trigger-inline-location");
            if (btnInlineLoc) {
                btnInlineLoc.addEventListener("click", function () {
                    openLocationFetchModal();
                });
            }

            initLocationModalEvents();

            if (!canPlace) {
                flash(status, "One or more items are out of stock.", true);
                document.getElementById("checkout-place").disabled = true;
            }
        } catch (error) {
            flash(status, "Could not load checkout preview.", true);
            return;
        }

        // Place Order Form Submission
        form.addEventListener("submit", async function (event) {
            event.preventDefault();
            var button = document.getElementById("checkout-place");

            var receiverName = (document.getElementById("checkout-selected-name")?.value || "").trim();
            var phone = (document.getElementById("checkout-selected-phone")?.value || "").trim();
            var addressLine = (document.getElementById("checkout-selected-address-line")?.value || "").trim();
            var city = (document.getElementById("checkout-selected-city")?.value || "").trim();
            var pincode = (document.getElementById("checkout-selected-pincode")?.value || "").trim();
            var addressType = document.getElementById("checkout-selected-address-type")?.value || "Home";
            var addressId = Number(document.getElementById("checkout-selected-address-id")?.value || 0);
            var lat = document.getElementById("checkout-selected-latitude")?.value ? Number(document.getElementById("checkout-selected-latitude").value) : null;
            var lon = document.getElementById("checkout-selected-longitude")?.value ? Number(document.getElementById("checkout-selected-longitude").value) : null;
            var shouldSaveAddress = document.getElementById("checkout-save-address")?.checked;

            if (!receiverName) {
                alert("Please enter the receiver's full name.");
                document.getElementById("checkout-selected-name")?.focus();
                return;
            }
            if (!phone || phone.length < 10) {
                alert("Please enter a valid 10-digit mobile number.");
                document.getElementById("checkout-selected-phone")?.focus();
                return;
            }
            if (!addressLine || addressLine.length < 5) {
                alert("Please enter a complete delivery address (street/flat).");
                document.getElementById("checkout-selected-address-line")?.focus();
                return;
            }
            if (!city) {
                alert("Please enter the city or district.");
                document.getElementById("checkout-selected-city")?.focus();
                return;
            }
            if (!pincode || pincode.length < 6) {
                alert("Please enter a valid 6-digit pincode.");
                document.getElementById("checkout-selected-pincode")?.focus();
                return;
            }

            var paymentMethod = "COD";
            var paymentRadios = document.getElementsByName("paymentMethod");
            for (var i = 0; i < paymentRadios.length; i++) {
                if (paymentRadios[i].checked) {
                    paymentMethod = paymentRadios[i].value;
                    break;
                }
            }

            button.disabled = true;
            flash(status, "");

            // If user opted to save/update address, save in background
            if (shouldSaveAddress) {
                fetch("/Checkout/SaveAddress", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        addressId: addressId,
                        addressType: addressType,
                        receiverName: receiverName,
                        phone: phone,
                        addressLine: addressLine,
                        city: city,
                        pincode: pincode,
                        latitude: lat,
                        longitude: lon,
                        isDefault: (addressId > 0 ? (savedAddressesList.find(function(a) { return (a.addressId || a.AddressId) === addressId; })?.isDefault ?? false) : (savedAddressesList.length === 0))
                    })
                }).catch(function(e) { console.warn("Save address background error", e); });
            }

            var orderData = {
                productVariantId: buyNow ? variantId : 0,
                quantity: buyNow ? quantity : 1,
                receiverName: receiverName,
                phone: phone,
                addressLine: addressLine,
                city: city,
                pincode: pincode,
                latitude: lat,
                longitude: lon,
                saveAddress: false,
                paymentMethod: paymentMethod
            };

            try {
                if (paymentMethod === "Razorpay") {
                    // Create Razorpay Order
                    var rpResponse = await fetch("/api/payment/create-order", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify(orderData)
                    });

                    if (window.smartCartHandleAuth && window.smartCartHandleAuth(rpResponse)) return;
                    if (!rpResponse.ok) {
                        var errData = await rpResponse.json();
                        flash(status, errData.message || "Failed to create payment order.", true);
                        button.disabled = false;
                        return;
                    }

                    var rpOrder = await rpResponse.json();

                    var options = {
                        key: rpOrder.keyId || rpOrder.KeyId,
                        amount: rpOrder.amount || rpOrder.Amount,
                        currency: rpOrder.currency || rpOrder.Currency,
                        name: "SmartCart",
                        description: "Purchase from SmartCart",
                        order_id: rpOrder.orderId || rpOrder.OrderId,
                        handler: async function (response) {
                            try {
                                var verifyPayload = {
                                    razorpayPaymentId: response.razorpay_payment_id,
                                    razorpayOrderId: response.razorpay_order_id,
                                    razorpaySignature: response.razorpay_signature,
                                    productVariantId: orderData.productVariantId,
                                    quantity: orderData.quantity,
                                    receiverName: orderData.receiverName,
                                    phone: orderData.phone,
                                    addressLine: orderData.addressLine,
                                    city: orderData.city,
                                    pincode: orderData.pincode
                                };

                                var verifyResponse = await fetch("/api/payment/verify-payment", {
                                    method: "POST",
                                    headers: { "Content-Type": "application/json" },
                                    body: JSON.stringify(verifyPayload)
                                });

                                var verifyResult = await verifyResponse.json();
                                if (verifyResult.success) {
                                    if (window.refreshSmartCartBag) window.refreshSmartCartBag();
                                    window.location.href = "/Checkout/Confirmation/" + verifyResult.orderId;
                                } else {
                                    flash(status, verifyResult.message || "Payment verification failed.", true);
                                    button.disabled = false;
                                }
                            } catch (e) {
                                flash(status, "An error occurred while finalizing payment.", true);
                                button.disabled = false;
                            }
                        },
                        modal: {
                            ondismiss: function () {
                                flash(status, "Payment was cancelled.", true);
                                button.disabled = false;
                            }
                        },
                        prefill: {
                            name: receiverName,
                            contact: phone
                        }
                    };

                    var rzp = new Razorpay(options);
                    rzp.on("payment.failed", function (response) {
                        flash(status, response.error.description || "Payment failed.", true);
                        button.disabled = false;
                    });
                    rzp.open();
                } else {
                    // Normal COD Checkout
                    var placeResponse = await fetch("/Checkout/Place", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify(orderData)
                    });

                    if (window.smartCartHandleAuth && window.smartCartHandleAuth(placeResponse)) return;
                    if (!placeResponse.ok) {
                        flash(status, "Could not place order.", true);
                        button.disabled = false;
                        return;
                    }

                    var result = await placeResponse.json();
                    if ((result.statusCode || result.StatusCode) && (result.responseCode || result.ResponseCode) > 0) {
                        if (window.refreshSmartCartBag) window.refreshSmartCartBag();
                        window.location.href = "/Checkout/Confirmation/" + (result.responseCode || result.ResponseCode);
                    } else {
                        flash(status, result.responseMsg || result.ResponseMsg || "Could not place order.", true);
                        button.disabled = false;
                    }
                }
            } catch (e) {
                flash(status, "An error occurred while placing order.", true);
                button.disabled = false;
            }
        });

        // Payment Method Select Highlight
        var payLabels = document.querySelectorAll(".shop-checkout-pay");
        payLabels.forEach(function (label) {
            var radio = label.querySelector('input[type="radio"]');
            if (radio) {
                radio.addEventListener("change", function () {
                    payLabels.forEach(function (l) { l.classList.remove("is-selected"); });
                    if (radio.checked) {
                        label.classList.add("is-selected");
                    }
                });
            }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initCheckout);
    } else {
        initCheckout();
    }
})();
