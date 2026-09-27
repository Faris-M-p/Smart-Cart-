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
    var ALLOWED_ADDRESS_TYPES = ["Home", "Work", "Office", "Other"];

    function openModal(target) {
        var modalEl = typeof target === "string" ? document.getElementById(target) : target;
        if (!modalEl) return;

        if (window.bootstrap && window.bootstrap.Modal) {
            try {
                var inst = window.bootstrap.Modal.getInstance(modalEl) || new window.bootstrap.Modal(modalEl);
                inst.show();
                return;
            } catch (e) {
                console.warn("Bootstrap modal failed, fallback to standalone modal", e);
            }
        }

        modalEl.classList.add("show");
        modalEl.style.display = "block";
        modalEl.removeAttribute("aria-hidden");
        modalEl.setAttribute("aria-modal", "true");

        var backdropId = modalEl.id + "-backdrop";
        var backdrop = document.getElementById(backdropId);
        if (!backdrop) {
            backdrop = document.createElement("div");
            backdrop.id = backdropId;
            backdrop.className = "modal-backdrop fade show";
            if (modalEl.id === "locationFetchModal") {
                backdrop.style.zIndex = "1055";
                modalEl.style.zIndex = "1060";
            }
            document.body.appendChild(backdrop);

            backdrop.addEventListener("click", function () {
                closeModal(modalEl);
            });
        }
        document.body.classList.add("modal-open");
        document.body.style.overflow = "hidden";
    }

    function closeModal(target) {
        var modalEl = typeof target === "string" ? document.getElementById(target) : target;
        if (!modalEl) return;

        if (window.bootstrap && window.bootstrap.Modal) {
            try {
                var inst = window.bootstrap.Modal.getInstance(modalEl);
                if (inst) {
                    inst.hide();
                    return;
                }
            } catch (e) { }
        }

        modalEl.classList.remove("show");
        modalEl.style.display = "none";
        modalEl.setAttribute("aria-hidden", "true");
        if (modalEl.style.zIndex) {
            modalEl.style.zIndex = "";
        }

        var backdropId = modalEl.id + "-backdrop";
        var backdrop = document.getElementById(backdropId);
        if (backdrop && backdrop.parentNode) {
            backdrop.parentNode.removeChild(backdrop);
        }

        if (!document.querySelector(".modal.show")) {
            document.body.classList.remove("modal-open");
            document.body.style.overflow = "";
        }
    }

    document.addEventListener("click", function (e) {
        var dismissBtn = e.target.closest('[data-bs-dismiss="modal"], .btn-close');
        if (dismissBtn) {
            var parentModal = dismissBtn.closest(".modal");
            if (parentModal) {
                closeModal(parentModal);
            }
            return;
        }

        var toggleBtn = e.target.closest('[data-bs-toggle="collapse"]');
        if (toggleBtn) {
            var targetId = toggleBtn.getAttribute("data-bs-target");
            var targetEl = targetId ? document.querySelector(targetId) : null;
            if (targetEl) {
                var isCollapsed = toggleBtn.classList.contains("collapsed");
                if (isCollapsed) {
                    toggleBtn.classList.remove("collapsed");
                    targetEl.classList.add("show");
                    targetEl.style.display = "block";
                } else {
                    toggleBtn.classList.add("collapsed");
                    targetEl.classList.remove("show");
                    targetEl.style.display = "none";
                }
            }
        }
    });

    // =========================================================================
    // 1. RENDER SAVED ADDRESSES (MAIN LONG CARD & DRAWER LIST)
    // =========================================================================
    function renderSavedAddressesGrid(addresses) {
        var selectedCard = document.getElementById("selected-address-card");
        var noAddrBox = document.getElementById("no-address-alert");
        var drawerListEl = document.getElementById("drawer-addresses-list");

        if (!addresses || !addresses.length) {
            if (selectedCard) selectedCard.hidden = true;
            if (noAddrBox) noAddrBox.hidden = false;
            if (drawerListEl) drawerListEl.innerHTML = '<p class="text-muted text-center py-3">No saved addresses found.</p>';
            return;
        }

        if (noAddrBox) noAddrBox.hidden = true;

        // Render Select Address Drawer Cards List
        if (drawerListEl) {
            var selectedId = Number(document.getElementById("checkout-selected-address-id").value || 0);

            drawerListEl.innerHTML = addresses.map(function (addr) {
                var id = addr.addressId || addr.AddressId;
                var type = escapeHtml(addr.addressType || addr.AddressType || "Home");
                var typeClass = "type-" + type.toLowerCase();
                var name = escapeHtml(addr.receiverName || addr.ReceiverName || "");
                var phone = escapeHtml(addr.phone || addr.Phone || "");
                var line = escapeHtml(addr.addressLine || addr.AddressLine || "");
                var city = escapeHtml(addr.city || addr.City || "");
                var pin = escapeHtml(addr.pincode || addr.Pincode || "");
                var isDefault = !!(addr.isDefault || addr.IsDefault);
                var isSel = (id === selectedId);

                return '<div class="shop-drawer-card ' + (isSel ? 'is-selected' : '') + '" data-address-id="' + id + '">' +
                    '<div class="shop-drawer-radio"></div>' +
                    '<div class="flex-1 min-w-0 pr-2">' +
                        '<div class="d-flex align-items-center gap-2 mb-1">' +
                            '<span class="shop-address-badge ' + typeClass + '">' + type + '</span>' +
                            (isDefault ? '<span class="shop-badge-default">Default</span>' : '') +
                            (isSel ? '<span class="text-xs text-danger font-semibold ms-auto">Selected</span>' : '') +
                        '</div>' +
                        '<div class="fw-bold text-dark text-sm">' + name + ' <span class="text-muted fw-normal ms-1">' + phone + '</span></div>' +
                        '<div class="text-muted text-xs mt-1 leading-snug">' + line + ', ' + city + ' - ' + pin + '</div>' +
                    '</div>' +
                    '<button type="button" class="shop-btn-card-edit" data-action="edit" data-id="' + id + '">Edit</button>' +
                '</div>';
            }).join("");

            // Attach event handlers to drawer cards
            drawerListEl.querySelectorAll(".shop-drawer-card").forEach(function (card) {
                var id = Number(card.getAttribute("data-address-id"));
                card.addEventListener("click", function (e) {
                    var action = e.target.getAttribute("data-action");
                    if (action === "edit") {
                        e.stopPropagation();
                        closeModal("selectAddressDrawer");
                        openAddressModal(id);
                        return;
                    }
                    selectSavedAddress(id);
                    closeModal("selectAddressDrawer");
                });
            });
        }
    }

    // Select an Address Card and bind to hidden checkout inputs
    function selectSavedAddress(id) {
        var addr = savedAddressesList.find(function (a) {
            return (a.addressId || a.AddressId) === id;
        });
        if (!addr) return;

        var addrId = addr.addressId || addr.AddressId;
        var type = addr.addressType || addr.AddressType || "Home";
        var name = addr.receiverName || addr.ReceiverName || "";
        var phone = addr.phone || addr.Phone || "";
        var line = addr.addressLine || addr.AddressLine || "";
        var city = addr.city || addr.City || "";
        var pin = addr.pincode || addr.Pincode || "";
        var isDefault = !!(addr.isDefault || addr.IsDefault);

        document.getElementById("checkout-selected-address-id").value = addrId;
        document.getElementById("checkout-selected-name").value = name;
        document.getElementById("checkout-selected-phone").value = phone;
        document.getElementById("checkout-selected-address-line").value = line;
        document.getElementById("checkout-selected-city").value = city;
        document.getElementById("checkout-selected-pincode").value = pin;
        document.getElementById("checkout-selected-address-type").value = type;
        document.getElementById("checkout-selected-latitude").value = addr.latitude || addr.Latitude || "";
        document.getElementById("checkout-selected-longitude").value = addr.longitude || addr.Longitude || "";

        // Update Main Selected Address Long Card
        var selectedCard = document.getElementById("selected-address-card");
        var nameEl = document.getElementById("selected-card-name");
        var phoneEl = document.getElementById("selected-card-phone");
        var lineEl = document.getElementById("selected-card-line");
        var typeEl = document.getElementById("selected-card-type");
        var defaultEl = document.getElementById("selected-card-default");

        if (selectedCard && nameEl && phoneEl && lineEl && typeEl) {
            nameEl.textContent = name;
            phoneEl.textContent = phone;
            lineEl.textContent = line + ", " + city + " - " + pin;
            typeEl.textContent = type.toUpperCase();
            typeEl.className = "shop-address-badge type-" + type.toLowerCase();
            if (defaultEl) defaultEl.hidden = !isDefault;
            selectedCard.hidden = false;
        }

        // Update Sidebar Delivery Preview
        var delPreview = document.getElementById("checkout-delivery-preview");
        var previewName = document.getElementById("preview-deliver-name");
        var previewAddr = document.getElementById("preview-deliver-address");
        if (delPreview && previewName && previewAddr) {
            previewName.textContent = name + " (" + type + ")";
            previewAddr.textContent = line + ", " + city + " - " + pin;
            delPreview.hidden = false;
        }

        // Highlight selected card in drawer
        document.querySelectorAll(".shop-drawer-card").forEach(function (card) {
            if (Number(card.getAttribute("data-address-id")) === addrId) {
                card.classList.add("is-selected");
            } else {
                card.classList.remove("is-selected");
            }
        });
    }

    // =========================================================================
    // 2. ADD / EDIT ADDRESS MODAL HANDLERS
    // =========================================================================
    window.openAddressModal = function (id) {
        var modalEl = document.getElementById("addressModal");
        if (!modalEl) return;

        var titleEl = document.getElementById("addressModalLabel");
        var form = document.getElementById("address-modal-form");
        if (form) form.reset();

        var existingTypes = savedAddressesList.map(function (a) {
            return (a.addressType || a.AddressType || "").toLowerCase();
        });

        if (id > 0) {
            var addr = savedAddressesList.find(function (a) { return (a.addressId || a.AddressId) === id; });
            if (addr) {
                var currentType = addr.addressType || addr.AddressType || "Home";
                if (titleEl) titleEl.textContent = "Edit " + currentType + " Address";
                document.getElementById("modal-address-id").value = id;
                document.getElementById("modal-name").value = addr.receiverName || addr.ReceiverName || "";
                document.getElementById("modal-phone").value = addr.phone || addr.Phone || "";
                document.getElementById("modal-address-line").value = addr.addressLine || addr.AddressLine || "";
                document.getElementById("modal-city").value = addr.city || addr.City || "";
                document.getElementById("modal-pincode").value = addr.pincode || addr.Pincode || "";
                document.getElementById("modal-latitude").value = addr.latitude || addr.Latitude || "";
                document.getElementById("modal-longitude").value = addr.longitude || addr.Longitude || "";
                document.getElementById("modal-address-type").value = currentType;
                document.querySelectorAll(".shop-type-pill").forEach(function (pill) {
                    pill.classList.toggle("active", (pill.getAttribute("data-type") || "").toLowerCase() === currentType.toLowerCase());
                });
                document.getElementById("modal-is-default").checked = !!(addr.isDefault || addr.IsDefault);
            }
        } else {
            // Find first available unsaved type among Home, Work, Office, Other
            var availableType = ALLOWED_ADDRESS_TYPES.find(function (t) {
                return !existingTypes.includes(t.toLowerCase());
            }) || "Home";

            var existingForAvailable = savedAddressesList.find(function (a) {
                return (a.addressType || a.AddressType || "").toLowerCase() === availableType.toLowerCase();
            });

            if (titleEl) titleEl.textContent = existingForAvailable ? ("Edit " + availableType + " Address") : ("Add " + availableType + " Address");
            document.getElementById("modal-address-id").value = existingForAvailable ? (existingForAvailable.addressId || existingForAvailable.AddressId) : "0";
            document.getElementById("modal-name").value = (existingForAvailable ? (existingForAvailable.receiverName || existingForAvailable.ReceiverName) : (customerProfile.fullName || customerProfile.FullName)) || "";
            document.getElementById("modal-phone").value = (existingForAvailable ? (existingForAvailable.phone || existingForAvailable.Phone) : (customerProfile.phone || customerProfile.Phone)) || "";
            document.getElementById("modal-address-line").value = existingForAvailable ? (existingForAvailable.addressLine || existingForAvailable.AddressLine) : "";
            document.getElementById("modal-city").value = existingForAvailable ? (existingForAvailable.city || existingForAvailable.City) : "";
            document.getElementById("modal-pincode").value = existingForAvailable ? (existingForAvailable.pincode || existingForAvailable.Pincode) : "";
            document.getElementById("modal-latitude").value = existingForAvailable ? (existingForAvailable.latitude || existingForAvailable.Latitude || "") : "";
            document.getElementById("modal-longitude").value = existingForAvailable ? (existingForAvailable.longitude || existingForAvailable.Longitude || "") : "";
            document.getElementById("modal-address-type").value = availableType;
            document.querySelectorAll(".shop-type-pill").forEach(function (pill) {
                pill.classList.toggle("active", (pill.getAttribute("data-type") || "").toLowerCase() === availableType.toLowerCase());
            });
            document.getElementById("modal-is-default").checked = existingForAvailable ? !!(existingForAvailable.isDefault || existingForAvailable.IsDefault) : (savedAddressesList.length === 0);
        }

        openModal(modalEl);
    };

    function initAddressModalEvents() {
        // Address Type Pill Selector
        document.querySelectorAll(".shop-type-pill").forEach(function (btn) {
            btn.addEventListener("click", function () {
                document.querySelectorAll(".shop-type-pill").forEach(function (b) { b.classList.remove("active"); });
                btn.classList.add("active");
                var type = btn.getAttribute("data-type") || "Home";
                document.getElementById("modal-address-type").value = type;

                // Check if user already has an address of this type
                var existingAddr = savedAddressesList.find(function (a) {
                    return (a.addressType || a.AddressType || "").toLowerCase() === type.toLowerCase();
                });

                var titleEl = document.getElementById("addressModalLabel");
                if (existingAddr) {
                    document.getElementById("modal-address-id").value = existingAddr.addressId || existingAddr.AddressId;
                    if (titleEl) titleEl.textContent = "Edit " + type + " Address";
                    document.getElementById("modal-name").value = existingAddr.receiverName || existingAddr.ReceiverName || "";
                    document.getElementById("modal-phone").value = existingAddr.phone || existingAddr.Phone || "";
                    document.getElementById("modal-address-line").value = existingAddr.addressLine || existingAddr.AddressLine || "";
                    document.getElementById("modal-city").value = existingAddr.city || existingAddr.City || "";
                    document.getElementById("modal-pincode").value = existingAddr.pincode || existingAddr.Pincode || "";
                    document.getElementById("modal-latitude").value = existingAddr.latitude || existingAddr.Latitude || "";
                    document.getElementById("modal-longitude").value = existingAddr.longitude || existingAddr.Longitude || "";
                    document.getElementById("modal-is-default").checked = !!(existingAddr.isDefault || existingAddr.IsDefault);
                } else {
                    document.getElementById("modal-address-id").value = "0";
                    if (titleEl) titleEl.textContent = "Add " + type + " Address";
                }
            });
        });

        // Save Address Modal Action
        var btnSave = document.getElementById("btn-save-address-modal");
        if (btnSave) {
            btnSave.addEventListener("click", async function () {
                var addressId = Number(document.getElementById("modal-address-id").value || 0);
                var receiverName = (document.getElementById("modal-name").value || "").trim();
                var phone = (document.getElementById("modal-phone").value || "").trim();
                var addressLine = (document.getElementById("modal-address-line").value || "").trim();
                var city = (document.getElementById("modal-city").value || "").trim();
                var pincode = (document.getElementById("modal-pincode").value || "").trim();
                var addressType = document.getElementById("modal-address-type").value || "Home";
                var lat = document.getElementById("modal-latitude").value ? Number(document.getElementById("modal-latitude").value) : null;
                var lon = document.getElementById("modal-longitude").value ? Number(document.getElementById("modal-longitude").value) : null;
                var isDefault = document.getElementById("modal-is-default").checked;

                if (!receiverName) { alert("Please enter full name."); return; }
                if (!phone || phone.length < 10) { alert("Please enter a valid 10-digit mobile number."); return; }
                if (!addressLine) { alert("Please enter address line."); return; }
                if (!city) { alert("Please enter city."); return; }
                if (!pincode || pincode.length < 6) { alert("Please enter a valid 6-digit pincode."); return; }

                btnSave.disabled = true;

                try {
                    var response = await fetch("/Checkout/SaveAddress", {
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
                            isDefault: isDefault
                        })
                    });

                    var result = await response.json();
                    if (result.statusCode || result.StatusCode) {
                        var savedId = result.responseCode || result.ResponseCode || addressId;
                        closeModal("addressModal");

                        // Refresh Addresses List
                        await reloadUserAddresses(savedId);
                    } else {
                        alert(result.responseMsg || result.ResponseMsg || "Could not save address.");
                    }
                } catch (e) {
                    alert("Error saving address. Please try again.");
                } finally {
                    btnSave.disabled = false;
                }
            });
        }

        // Trigger Location Fetch Modal from Address Modal
        var btnLocFetch = document.getElementById("btn-trigger-location-fetch");
        if (btnLocFetch) {
            btnLocFetch.addEventListener("click", function () {
                openLocationFetchModal();
            });
        }
    }

    async function reloadUserAddresses(selectId) {
        try {
            var res = await fetch("/Checkout/GetAddresses");
            if (res.ok) {
                var list = await res.json();
                savedAddressesList = list || [];
                renderSavedAddressesGrid(savedAddressesList);
                if (savedAddressesList.length > 0) {
                    var targetId = selectId || savedAddressesList[0].addressId || savedAddressesList[0].AddressId;
                    selectSavedAddress(targetId);
                }
            }
        } catch (e) {
            console.error("Reload addresses failed", e);
        }
    }

    // =========================================================================
    // 3. LOCATION FETCH MODAL (LEAFLET OPENSTREETMAP INTERACTIVE MAP)
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

        var defaultLat = 11.2588; // Kozhikode default lat
        var defaultLon = 75.7804; // Kozhikode default lon

        if (!leafletMap) {
            leafletMap = L.map("osm-map").setView([defaultLat, defaultLon], 14);

            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
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
                statusText.innerHTML = '<span class="shop-dot-live">●</span> GPS Location Captured · Accurate within 5 meters';
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

                if (tempFetchedLocation.city) document.getElementById("modal-city").value = tempFetchedLocation.city;
                if (tempFetchedLocation.pincode) document.getElementById("modal-pincode").value = tempFetchedLocation.pincode;
                if (tempFetchedLocation.road && !document.getElementById("modal-address-line").value) {
                    document.getElementById("modal-address-line").value = tempFetchedLocation.road;
                }
                if (tempFetchedLocation.latitude) document.getElementById("modal-latitude").value = tempFetchedLocation.latitude;
                if (tempFetchedLocation.longitude) document.getElementById("modal-longitude").value = tempFetchedLocation.longitude;

                closeModal("locationFetchModal");
            });
        }

        // Search location via Nominatim API
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
                    alert("No matching location found. Try searching with city or landmark name.");
                }
            })
            .catch(function (e) {
                console.error("Search failed", e);
            });
        }

        if (btnSearch) {
            btnSearch.addEventListener("click", performLocationSearch);
        }
        if (inputSearch) {
            inputSearch.addEventListener("keydown", function (e) {
                if (e.key === "Enter") {
                    e.preventDefault();
                    performLocationSearch();
                }
            });
        }

        // Recenter GPS button
        var btnRecenter = document.getElementById("btn-recenter-gps");
        if (btnRecenter) {
            btnRecenter.addEventListener("click", function () {
                startGeolocationDetection();
            });
        }

        // Manual Location Selectors Event Listeners
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
    // 4. MAIN CHECKOUT INITIALIZATION & PLACE ORDER
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

            // Render Addresses Grid & Drawer List
            renderSavedAddressesGrid(savedAddressesList);

            if (savedAddressesList.length > 0) {
                var defaultAddr = savedAddressesList.find(function (a) { return a.isDefault || a.IsDefault; }) || savedAddressesList[0];
                selectSavedAddress(defaultAddr.addressId || defaultAddr.AddressId);
            }

            // Bind Change & Switch buttons to open Select Address Drawer
            var btnChange = document.getElementById("btn-trigger-address-drawer");
            if (btnChange) {
                btnChange.addEventListener("click", function () {
                    openModal("selectAddressDrawer");
                });
            }

            var btnSwitch = document.getElementById("btn-switch-address");
            if (btnSwitch) {
                btnSwitch.addEventListener("click", function () {
                    openModal("selectAddressDrawer");
                });
            }

            // Bind Edit Selected Address
            var btnEditSel = document.getElementById("btn-edit-selected-address");
            if (btnEditSel) {
                btnEditSel.addEventListener("click", function () {
                    var selectedId = Number(document.getElementById("checkout-selected-address-id").value || 0);
                    openAddressModal(selectedId);
                });
            }

            // Bind Add Address Buttons
            var btnOpenAdd = document.getElementById("btn-open-add-modal");
            if (btnOpenAdd) {
                btnOpenAdd.addEventListener("click", function () {
                    openAddressModal(0);
                });
            }

            var btnDrawerAdd = document.getElementById("btn-drawer-add-new");
            if (btnDrawerAdd) {
                btnDrawerAdd.addEventListener("click", function () {
                    closeModal("selectAddressDrawer");
                    openAddressModal(0);
                });
            }

            initAddressModalEvents();
            initLocationModalEvents();

            if (!canPlace) {
                flash(status, "One or more items are out of stock.", true);
                document.getElementById("checkout-place").disabled = true;
            }
        } catch (error) {
            flash(status, "Could not load checkout.", true);
            return;
        }

        // Place Order Form Submit
        form.addEventListener("submit", async function (event) {
            event.preventDefault();
            var button = document.getElementById("checkout-place");

            var addressId = Number(document.getElementById("checkout-selected-address-id").value || 0);
            var receiverName = document.getElementById("checkout-selected-name").value;
            var phone = document.getElementById("checkout-selected-phone").value;
            var addressLine = document.getElementById("checkout-selected-address-line").value;
            var city = document.getElementById("checkout-selected-city").value;
            var pincode = document.getElementById("checkout-selected-pincode").value;

            if (!addressId && !receiverName) {
                alert("Please select or add a delivery address before placing your order.");
                openAddressModal(0);
                return;
            }

            button.disabled = true;

            try {
                var resultResponse = await fetch("/Checkout/Place", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        productVariantId: buyNow ? variantId : 0,
                        quantity: buyNow ? quantity : 1,
                        addressId: addressId,
                        addressType: document.getElementById("checkout-selected-address-type").value || "Home",
                        receiverName: receiverName,
                        phone: phone,
                        addressLine: addressLine,
                        city: city,
                        pincode: pincode,
                        latitude: document.getElementById("checkout-selected-latitude").value ? Number(document.getElementById("checkout-selected-latitude").value) : null,
                        longitude: document.getElementById("checkout-selected-longitude").value ? Number(document.getElementById("checkout-selected-longitude").value) : null,
                        saveAddress: false,
                        paymentMethod: "COD"
                    })
                });

                if (window.smartCartHandleAuth && window.smartCartHandleAuth(resultResponse)) return;

                var result = await resultResponse.json();
                if (!(result.statusCode || result.StatusCode)) {
                    flash(status, result.responseMsg || result.ResponseMsg || "Could not place the order.", true);
                    button.disabled = false;
                    return;
                }
                if (window.refreshSmartCartBag) {
                    window.refreshSmartCartBag();
                }
                window.location.href = "/Checkout/Confirmation/" + (result.responseCode || result.ResponseCode);
            } catch (error) {
                flash(status, "Could not place the order.", true);
                button.disabled = false;
            }
        });
    }

    async function initConfirmation() {
        var page = document.querySelector("[data-order-id]");
        if (!page || document.getElementById("checkout-form")) return;

        var orderId = Number(page.getAttribute("data-order-id") || 0);
        var content = document.getElementById("confirm-content");
        var missing = document.getElementById("confirm-missing");
        var status = document.getElementById("confirm-status");
        if (!orderId) {
            if (missing) missing.hidden = false;
            return;
        }

        try {
            var response = await fetch("/Checkout/GetOrder?orderId=" + encodeURIComponent(orderId));
            if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) return;
            if (!response.ok) throw new Error("missing");

            var data = await response.json();
            var order = data.order || data.Order || {};
            var items = data.items || data.Items || [];
            var method = (order.paymentMethod || order.PaymentMethod || "COD") === "COD"
                ? "Cash on Delivery"
                : (order.paymentMethod || order.PaymentMethod);
            var payStatus = order.paymentStatus || order.PaymentStatus || "Pending";

            document.getElementById("confirm-number").textContent = "Order " + (order.orderNumber || order.OrderNumber || orderId);
            document.getElementById("confirm-meta").textContent =
                (order.orderStatus || order.OrderStatus || "Placed") +
                " · " + new Date(order.orderDate || order.OrderDate || Date.now()).toLocaleString("en-IN");
            document.getElementById("confirm-pay").textContent = method + " · " + payStatus;
            document.getElementById("confirm-address").innerHTML =
                "<strong>Deliver to</strong><br>" +
                escapeHtml(order.receiverName || order.ReceiverName) + "<br>" +
                escapeHtml(order.phone || order.Phone) + "<br>" +
                escapeHtml(order.addressLine || order.AddressLine) + "<br>" +
                escapeHtml(order.city || order.City) + " - " + escapeHtml(order.pincode || order.Pincode);
            document.getElementById("confirm-total").textContent = money(order.totalAmount || order.TotalAmount);
            renderItems(document.getElementById("confirm-items"), items);
            var orderLink = document.getElementById("confirm-order-link");
            if (orderLink) {
                orderLink.href = "/Orders/Details/" + encodeURIComponent(order.orderId || order.OrderId || orderId);
            }
            if (missing) missing.hidden = true;
            if (content) content.hidden = false;
            if (window.refreshSmartCartBag) {
                window.refreshSmartCartBag();
            }
        } catch (error) {
            if (content) content.hidden = true;
            if (missing) missing.hidden = false;
            flash(status, "Could not load this order.", true);
        }
    }

    initCheckout();
    initConfirmation();
})();
