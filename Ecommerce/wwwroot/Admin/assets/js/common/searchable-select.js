(function (window) {
    function escapeHtml(text) {
        return String(text == null ? '' : text)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function initials(name) {
        var text = String(name || '').trim();
        return text ? text.substring(0, 2) : '—';
    }

    function customProps(data) {
        var props = data && data.customProperties;
        if (!props) {
            return {};
        }
        if (typeof props === 'string') {
            try {
                return JSON.parse(props);
            } catch (e) {
                return {};
            }
        }
        return props;
    }

    function mediaLabel(data) {
        var props = customProps(data);
        var name = props.name || data.label || '';
        var imageUrl = props.imageUrl || '';
        if (!data.value || data.placeholder) {
            return escapeHtml(data.label || name);
        }

        var media = imageUrl
            ? '<img class="select-entity-thumb" src="' + escapeHtml(imageUrl) + '" alt="">'
            : '<span class="select-entity-initials">' + escapeHtml(initials(name)) + '</span>';

        return '<span class="select-entity-item">' +
            media +
            '<span class="select-entity-name">' + escapeHtml(name) + '</span>' +
            '</span>';
    }

    function imageTemplates(strToEl) {
        return {
            item: function (config, data) {
                return strToEl(
                    '<div class="' + config.classNames.item + ' ' +
                    (data.highlighted ? config.classNames.highlightedState : config.classNames.itemSelectable) +
                    (data.placeholder ? ' ' + config.classNames.placeholder : '') +
                    '" data-item data-id="' + data.id + '" data-value="' + escapeHtml(String(data.value ?? '')) + '"' +
                    (data.active ? ' aria-selected="true"' : '') +
                    (data.disabled ? ' aria-disabled="true"' : '') + '>' +
                    mediaLabel(data) +
                    '</div>'
                );
            },
            choice: function (config, data) {
                var disabled = !!data.disabled;
                return strToEl(
                    '<div class="' + config.classNames.item + ' ' + config.classNames.itemChoice + ' ' +
                    (disabled ? config.classNames.itemDisabled : config.classNames.itemSelectable) +
                    (data.placeholder ? ' ' + config.classNames.placeholder : '') +
                    '" data-select-text="' + escapeHtml(config.itemSelectText || '') + '" data-choice ' +
                    (disabled ? 'data-choice-disabled aria-disabled="true"' : 'data-choice-selectable') +
                    ' data-id="' + data.id + '" data-value="' + escapeHtml(String(data.value ?? '')) +
                    '" id="' + escapeHtml(data.elementId || '') + '" role="option">' +
                    mediaLabel(data) +
                    '</div>'
                );
            }
        };
    }

    function fill(selectEl, items, options) {
        if (!selectEl) {
            return;
        }

        options = options || {};
        var selected = options.selected == null ? '' : String(options.selected);
        var placeholder = options.placeholder || '';

        if (selectEl.choicesInstance) {
            selectEl.choicesInstance.destroy();
            selectEl.choicesInstance = null;
        }

        selectEl.innerHTML = '';
        var placeholderOption = new Option(placeholder, '');
        placeholderOption.setAttribute('placeholder', '');
        if (!selected) {
            placeholderOption.selected = true;
        }
        selectEl.appendChild(placeholderOption);

        (items || []).forEach(function (item) {
            if (!item || item.value == null || item.value === '' || !item.label) {
                return;
            }

            var option = new Option(item.label, String(item.value));
            option.dataset.customProperties = JSON.stringify({
                name: item.label,
                imageUrl: item.imageUrl || ''
            });
            if (selected && String(item.value) === selected) {
                option.selected = true;
            }
            selectEl.appendChild(option);
        });

        if (typeof Choices === 'undefined') {
            return;
        }

        selectEl.choicesInstance = new Choices(selectEl, {
            searchEnabled: true,
            searchPlaceholderValue: options.searchPlaceholder || 'Search...',
            searchResultLimit: 50,
            itemSelectText: '',
            shouldSort: false,
            allowHTML: true,
            position: 'bottom',
            callbackOnCreateTemplates: imageTemplates
        });
    }

    function setValue(selectEl, value) {
        if (!selectEl) {
            return;
        }

        var next = value == null ? '' : String(value);
        if (selectEl.choicesInstance) {
            selectEl.choicesInstance.setChoiceByValue(next);
            return;
        }

        selectEl.value = next;
    }

    function getValue(selectEl) {
        if (!selectEl) {
            return '';
        }

        if (selectEl.choicesInstance) {
            var value = selectEl.choicesInstance.getValue(true);
            return value == null ? '' : String(value);
        }

        return selectEl.value || '';
    }

    window.adminSelect = {
        fill: fill,
        setValue: setValue,
        getValue: getValue
    };
})(window);
