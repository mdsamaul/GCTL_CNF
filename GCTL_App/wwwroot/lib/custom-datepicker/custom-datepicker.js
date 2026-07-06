// /*!
//  * custom Custom Flatpickr Wrapper
//  * ------------------------------
//  * Usage: <input class="custom-datepicker" data-holidays='[{"date":"2026-07-06","name":"Eid Holiday"}]' />
//  * Ba globally window.custom_HOLIDAYS = [{date:'2026-07-06', name:'Eid Holiday'}, ...]; diye o dite paren.
//  */
// (function (window, $) {
//     'use strict';

//     var DEFAULTS = {
//         displayFormat: 'd/m/Y',
//         backendFormat: 'Y-m-d',
//         holidays: []
//     };

//     function parseHolidays($el) {
//         var attrHolidays = $el.attr('data-holidays');
//         var holidays = [];
//         try {
//             if (attrHolidays) holidays = JSON.parse(attrHolidays);
//         } catch (e) {
//             console.warn('custom-datepicker: data-holidays JSON parse error', e);
//         }
//         if (!holidays.length && Array.isArray(window.custom_HOLIDAYS)) {
//             holidays = window.custom_HOLIDAYS;
//         }
//         return holidays;
//     }

//     function findHoliday(holidays, dateObj) {
//         var y = dateObj.getFullYear();
//         var m = String(dateObj.getMonth() + 1).padStart(2, '0');
//         var d = String(dateObj.getDate()).padStart(2, '0');
//         var iso = y + '-' + m + '-' + d;
//         return holidays.find(function (h) { return h.date === iso; });
//     }

//     function hideEmptyTrailingRow(fpInstance) {
//         setTimeout(function () {
//             var dayContainer = fpInstance.days;
//             var days = Array.prototype.slice.call(dayContainer.querySelectorAll('.flatpickr-day'));

//             if (!days.length) return;

//             days.forEach(function (d) {
//                 d.style.removeProperty('visibility');
//                 d.style.removeProperty('display');
//             });

//             var perRow = 7;
//             var totalCells = days.length;
//             var totalRows = Math.ceil(totalCells / perRow);

//             for (var r = 0; r < totalRows; r++) {
//                 var start = r * perRow;
//                 var end = start + perRow;
//                 var rowDays = days.slice(start, end);

//                 var hasCurrentMonthDay = rowDays.some(function (d) {
//                     return !d.classList.contains('prevMonthDay') && !d.classList.contains('nextMonthDay');
//                 });

//                 if (!hasCurrentMonthDay) {
//                     rowDays.forEach(function (d) {
//                         d.style.display = 'none';
//                     });
//                 } else {
//                     rowDays.forEach(function (d) {
//                         d.style.display = '';
//                         if (d.classList.contains('prevMonthDay') || d.classList.contains('nextMonthDay')) {
//                             d.style.visibility = 'hidden';
//                         } else {
//                             d.style.removeProperty('visibility');
//                         }
//                     });
//                 }
//             }
//         }, 20);
//     }

//     function addClearButton(fpInstance, $el) {
//         if (fpInstance.calendarContainer.querySelector('.custom-fp-clear-btn')) return;

//         var footer = document.createElement('div');
//         footer.className = 'custom-fp-footer';

//         var clearBtn = document.createElement('button');
//         clearBtn.type = 'button';
//         clearBtn.className = 'custom-fp-clear-btn';
//         clearBtn.textContent = 'Clear';
//         clearBtn.addEventListener('click', function (e) {
//             e.preventDefault();
//             e.stopPropagation();
//             fpInstance.clear();
//             fpInstance.close();
//         });

//         var todayBtn = document.createElement('button');
//         todayBtn.type = 'button';
//         todayBtn.className = 'custom-fp-today-btn';
//         todayBtn.textContent = 'Today';
//         todayBtn.addEventListener('click', function (e) {
//             e.preventDefault();
//             e.stopPropagation();
//             fpInstance.setDate(new Date(), true);
//             fpInstance.close();
//         });

//         footer.appendChild(todayBtn);
//         footer.appendChild(clearBtn);
//         fpInstance.calendarContainer.appendChild(footer);
//     }

//     function initOne(el) {
//         var $el = $(el);
//         if ($el.data('customFpInstance')) return;

//         var backendFormat = $el.data('backend-format') || DEFAULTS.backendFormat;
//         var displayFormat = $el.data('display-format') || DEFAULTS.displayFormat;
//         var holidays = parseHolidays($el);
//         var allowInput = $el.data('allow-input') !== false;
//         var minDate = $el.data('min-date') || null;
//         var maxDate = $el.data('max-date') || null;

//         var fp = flatpickr(el, {
//             altInput: true,
//             altFormat: displayFormat,
//             dateFormat: backendFormat,
//             allowInput: allowInput,
//             animate: false,
//             monthSelectorType: 'dropdown',
//             minDate: minDate,
//             maxDate: maxDate,
//             locale: { firstDayOfWeek: 0 },

//             onDayCreate: function (dObj, dStr, fpInstance, dayElem) {
//                 var dayDate = dayElem.dateObj;
//                 var currentHolidays = Array.isArray(window.custom_HOLIDAYS) ? window.custom_HOLIDAYS : holidays;
//                 var holiday = findHoliday(currentHolidays, dayDate);
//                 if (holiday) {
//                     dayElem.classList.add('custom-holiday');
//                     var tooltip = document.createElement('span');
//                     tooltip.className = 'custom-holiday-tooltip';
//                     tooltip.textContent = holiday.name;
//                     dayElem.appendChild(tooltip);
//                 }
//             },

//             onMonthChange: function (selectedDates, dateStr, fpInstance) {
//                 hideEmptyTrailingRow(fpInstance);
//                 refreshHolidaysForView(fpInstance, $el);
//             },

//             onYearChange: function (selectedDates, dateStr, fpInstance) {
//                 hideEmptyTrailingRow(fpInstance);
//                 refreshHolidaysForView(fpInstance, $el);
//             },

//             onReady: function (selectedDates, dateStr, fpInstance) {
//                 hideEmptyTrailingRow(fpInstance);
//                 addClearButton(fpInstance, $el);
//             },

//             onOpen: function (selectedDates, dateStr, fpInstance) {
//                 fpInstance.calendarContainer.style.transformOrigin = 'top center';
//                 hideEmptyTrailingRow(fpInstance);
//             }

//         });

//         $el.data('customFpInstance', fp);
//     }

//     function initAll(selector) {
//         $(selector || '.custom-datepicker').each(function () {
//             initOne(this);
//         });
//     }

//     function setDatepickerValue(selector, value) {
//         var $el = $(selector);
//         var fp = $el.data('customFpInstance');
//         if (!fp) {
//             $el.val(value);
//             return;
//         }
//         if (!value) {
//             fp.clear();
//             return;
//         }
//         fp.setDate(value, true);
//     }

//     function getDatepickerValue(selector) {
//         var $el = $(selector);
//         var fp = $el.data('customFpInstance');
//         if (!fp) return $el.val();
//         return $el.val();
//     }

//     function destroyDatepicker(selector) {
//         var $el = $(selector);
//         var fp = $el.data('customFpInstance');
//         if (fp) {
//             fp.destroy();
//             $el.removeData('customFpInstance');
//         }
//     }
//     function refreshHolidaysForView(fpInstance, $el) {
//         if (window.customHolidayService && typeof window.customHolidayService.refresh === 'function') {
//             var viewYear = fpInstance.currentYear;

//             // Already cached thakle synchronous re-render, network wait lagbe na
//             var existing = window.customHolidayService.getHolidays();
//             var alreadyHasYear = existing.some(function (h) {
//                 return h.date.startsWith(viewYear + '-');
//             });

//             if (alreadyHasYear) {
//                 fpInstance.redraw();
//                 return;
//             }

//             window.customHolidayService.refresh({ year: viewYear }).then(function () {
//                 fpInstance.redraw();
//             });
//         }
//     }

//     $(function () {
//         initAll('.custom-datepicker');
//     });

//     window.custom_Datepicker = {
//         init: initOne,
//         initAll: initAll,
//         setValue: setDatepickerValue,
//         getValue: getDatepickerValue,
//         destroy: destroyDatepicker
//     };

// })(window, window.jQuery);

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
            var totalRows = Math.ceil(days.length / perRow);

            for (var r = 0; r < totalRows; r++) {
                var rowDays = days.slice(r * perRow, r * perRow + perRow);
                var hasCurrentMonthDay = rowDays.some(function (d) {
                    return !d.classList.contains('prevMonthDay') && !d.classList.contains('nextMonthDay');
                });

                if (!hasCurrentMonthDay) {
                    rowDays.forEach(function (d) { d.style.display = 'none'; });
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
        }, 50);
    }

    function addClearButton(fpInstance) {
        if (fpInstance.calendarContainer.querySelector('.custom-fp-clear-btn')) return;

        var footer = document.createElement('div');
        footer.className = 'custom-fp-footer';

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

        footer.appendChild(todayBtn);
        footer.appendChild(clearBtn);
        fpInstance.calendarContainer.appendChild(footer);
    }

    function refreshHolidaysForView(fpInstance) {
        if (!window.customHolidayService || typeof window.customHolidayService.refresh !== 'function') return;

        var viewYear = fpInstance.currentYear;
        var existing = window.customHolidayService.getHolidays();
        var alreadyHasYear = existing.some(function (h) {
            return h.date.indexOf(viewYear + '-') === 0;
        });

        if (alreadyHasYear) {
            fpInstance.redraw();
            return;
        }

        window.customHolidayService.refresh({ year: viewYear }).then(function () {
            fpInstance.redraw();
        });
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
                var currentHolidays = Array.isArray(window.custom_HOLIDAYS) ? window.custom_HOLIDAYS : holidays;
                var holiday = findHoliday(currentHolidays, dayDate);
                if (holiday) {
                    dayElem.classList.add('custom-holiday');

                    var tooltip = document.createElement('span');
                    tooltip.className = 'custom-holiday-tooltip';
                    tooltip.textContent = holiday.name;
                    document.body.appendChild(tooltip);

                    // dayElem remove/destroy hole tooltip o remove kora
                    var cleanup = function () {
                        if (tooltip && tooltip.parentNode) tooltip.parentNode.removeChild(tooltip);
                    };
                    var observer = new MutationObserver(function (mutations) {
                        mutations.forEach(function (m) {
                            m.removedNodes.forEach(function (node) {
                                if (node === dayElem) {
                                    cleanup();
                                    observer.disconnect();
                                }
                            });
                        });
                    });
                    if (dayElem.parentNode) {
                        observer.observe(dayElem.parentNode, { childList: true });
                    }

                    dayElem.addEventListener('mouseenter', function () {
                        var dayRect = dayElem.getBoundingClientRect();
                        tooltip.classList.remove('tt-top', 'tt-bottom');
                        tooltip.style.visibility = 'hidden';
                        tooltip.style.opacity = '0';
                        tooltip.style.display = 'block';

                        var ttRect = tooltip.getBoundingClientRect();
                        var vw = window.innerWidth;
                        var vh = window.innerHeight;

                        var left = dayRect.left + (dayRect.width / 2) - (ttRect.width / 2);
                        var top = dayRect.top - ttRect.height - 8;
                        var placement = 'tt-top';

                        if (top < 4) {
                            top = dayRect.bottom + 8;
                            placement = 'tt-bottom';
                        }
                        if (left < 4) left = 4;
                        if (left + ttRect.width > vw - 4) left = vw - ttRect.width - 4;
                        if (placement === 'tt-bottom' && top + ttRect.height > vh - 4) {
                            top = vh - ttRect.height - 4;
                        }

                        tooltip.style.left = left + 'px';
                        tooltip.style.top = top + 'px';
                        tooltip.classList.add(placement, 'tt-visible');
                    });

                    dayElem.addEventListener('mouseleave', function () {
                        tooltip.classList.remove('tt-visible');
                    });
                }
            },
            onMonthChange: function (selectedDates, dateStr, fpInstance) {
                document.querySelectorAll('.custom-holiday-tooltip').forEach(function (t) { t.remove(); });
                hideEmptyTrailingRow(fpInstance);
                refreshHolidaysForView(fpInstance);
            },

            onYearChange: function (selectedDates, dateStr, fpInstance) {
                document.querySelectorAll('.custom-holiday-tooltip').forEach(function (t) { t.remove(); });
                hideEmptyTrailingRow(fpInstance);
                refreshHolidaysForView(fpInstance);
            },

            onReady: function (selectedDates, dateStr, fpInstance) {
                hideEmptyTrailingRow(fpInstance);
                addClearButton(fpInstance);
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
        if (!fp) { $el.val(value); return; }
        if (!value) { fp.clear(); return; }
        fp.setDate(value, true);
    }

    function getDatepickerValue(selector) {
        var $el = $(selector);
        return $el.val();
    }

    function destroyDatepicker(selector) {
        var $el = $(selector);
        var fp = $el.data('customFpInstance');
        if (fp) {
            document.querySelectorAll('.custom-holiday-tooltip').forEach(function (t) {
                if (fp.calendarContainer && !fp.calendarContainer.contains(t)) {
                    t.remove();
                }
            });
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