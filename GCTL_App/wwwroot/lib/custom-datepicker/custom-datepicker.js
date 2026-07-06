/*!
 * custom Custom Flatpickr Wrapper
 * ------------------------------
 * Usage: <input class="custom-datepicker" data-holidays='[{"date":"2026-07-06","name":"Eid Holiday"}]' />
 * Ba globally window.custom_HOLIDAYS = [{date:'2026-07-06', name:'Eid Holiday'}, ...]; diye o dite paren.
 */
(function (window, $) {
    'use strict';

    var DEFAULTS = {
        displayFormat: 'd/m/Y',
        backendFormat: 'Y-m-d',
        holidays: []
    };

    function parseHolidays($el) {
        var attrHolidays = $el.attr('data-holidays');
        var holidays = [];
        try {
            if (attrHolidays) holidays = JSON.parse(attrHolidays);
        } catch (e) {
            console.warn('custom-datepicker: data-holidays JSON parse error', e);
        }
        if (!holidays.length && Array.isArray(window.custom_HOLIDAYS)) {
            holidays = window.custom_HOLIDAYS;
        }
        return holidays;
    }

    function findHoliday(holidays, dateObj) {
        var y = dateObj.getFullYear();
        var m = String(dateObj.getMonth() + 1).padStart(2, '0');
        var d = String(dateObj.getDate()).padStart(2, '0');
        var iso = y + '-' + m + '-' + d;
        return holidays.find(function (h) { return h.date === iso; });
    }

    function hideEmptyTrailingRow(fpInstance) {
        setTimeout(function () {
            var dayContainer = fpInstance.days;
            var days = Array.prototype.slice.call(dayContainer.querySelectorAll('.flatpickr-day'));

            if (!days.length) return;

            days.forEach(function (d) {
                d.style.removeProperty('visibility');
                d.style.removeProperty('display');
            });

            var perRow = 7;
            var totalCells = days.length;
            var totalRows = Math.ceil(totalCells / perRow);

            for (var r = 0; r < totalRows; r++) {
                var start = r * perRow;
                var end = start + perRow;
                var rowDays = days.slice(start, end);

                var hasCurrentMonthDay = rowDays.some(function (d) {
                    return !d.classList.contains('prevMonthDay') && !d.classList.contains('nextMonthDay');
                });

                if (!hasCurrentMonthDay) {
                    rowDays.forEach(function (d) {
                        d.style.display = 'none';
                    });
                } else {
                    rowDays.forEach(function (d) {
                        d.style.display = '';
                        if (d.classList.contains('prevMonthDay') || d.classList.contains('nextMonthDay')) {
                            d.style.visibility = 'hidden';
                        } else {
                            d.style.removeProperty('visibility');
                        }
                    });
                }
            }
        }, 20);
    }

    function addClearButton(fpInstance, $el) {
        if (fpInstance.calendarContainer.querySelector('.custom-fp-clear-btn')) return;

        var footer = document.createElement('div');
        footer.className = 'custom-fp-footer';

        var clearBtn = document.createElement('button');
        clearBtn.type = 'button';
        clearBtn.className = 'custom-fp-clear-btn';
        clearBtn.textContent = 'Clear';
        clearBtn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            fpInstance.clear();
            fpInstance.close();
        });

        var todayBtn = document.createElement('button');
        todayBtn.type = 'button';
        todayBtn.className = 'custom-fp-today-btn';
        todayBtn.textContent = 'Today';
        todayBtn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            fpInstance.setDate(new Date(), true);
            fpInstance.close();
        });

        footer.appendChild(todayBtn);
        footer.appendChild(clearBtn);
        fpInstance.calendarContainer.appendChild(footer);
    }

    function initOne(el) {
        var $el = $(el);
        if ($el.data('customFpInstance')) return;

        var backendFormat = $el.data('backend-format') || DEFAULTS.backendFormat;
        var displayFormat = $el.data('display-format') || DEFAULTS.displayFormat;
        var holidays = parseHolidays($el);
        var allowInput = $el.data('allow-input') !== false;
        var minDate = $el.data('min-date') || null;
        var maxDate = $el.data('max-date') || null;

        var fp = flatpickr(el, {
            altInput: true,
            altFormat: displayFormat,
            dateFormat: backendFormat,
            allowInput: allowInput,
            animate: false,
            monthSelectorType: 'dropdown',
            minDate: minDate,
            maxDate: maxDate,
            locale: { firstDayOfWeek: 0 },

            onDayCreate: function (dObj, dStr, fpInstance, dayElem) {
                var dayDate = dayElem.dateObj;
                var holiday = findHoliday(holidays, dayDate);
                if (holiday) {
                    dayElem.classList.add('custom-holiday');
                    var tooltip = document.createElement('span');
                    tooltip.className = 'custom-holiday-tooltip';
                    tooltip.textContent = holiday.name;
                    dayElem.appendChild(tooltip);
                }
            },

            onMonthChange: function (selectedDates, dateStr, fpInstance) {
                hideEmptyTrailingRow(fpInstance);
            },

            onYearChange: function (selectedDates, dateStr, fpInstance) {
                hideEmptyTrailingRow(fpInstance);
            },

            onReady: function (selectedDates, dateStr, fpInstance) {
                hideEmptyTrailingRow(fpInstance);
                addClearButton(fpInstance, $el);
            },

            onOpen: function (selectedDates, dateStr, fpInstance) {
                fpInstance.calendarContainer.style.transformOrigin = 'top center';
                hideEmptyTrailingRow(fpInstance);
            }

        });

        $el.data('customFpInstance', fp);
    }

    function initAll(selector) {
        $(selector || '.custom-datepicker').each(function () {
            initOne(this);
        });
    }

    function setDatepickerValue(selector, value) {
        var $el = $(selector);
        var fp = $el.data('customFpInstance');
        if (!fp) {
            $el.val(value);
            return;
        }
        if (!value) {
            fp.clear();
            return;
        }
        fp.setDate(value, true);
    }

    function getDatepickerValue(selector) {
        var $el = $(selector);
        var fp = $el.data('customFpInstance');
        if (!fp) return $el.val();
        return $el.val();
    }

    function destroyDatepicker(selector) {
        var $el = $(selector);
        var fp = $el.data('customFpInstance');
        if (fp) {
            fp.destroy();
            $el.removeData('customFpInstance');
        }
    }

    $(function () {
        initAll('.custom-datepicker');
    });

    window.custom_Datepicker = {
        init: initOne,
        initAll: initAll,
        setValue: setDatepickerValue,
        getValue: getDatepickerValue,
        destroy: destroyDatepicker
    };

})(window, window.jQuery);