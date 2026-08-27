const ExtraDatetimeJs = (() => {
    const DIGIT_REGEX = /^\d$/;

    const getLang = () => {
        const langAttr = $('html').attr('lang');
        return typeof langAttr === 'undefined' || langAttr === '' ? 'vi' : langAttr;
    };

    const getFormat = () => (getLang() === 'en' ? 'MM/DD/YYYY' : 'DD/MM/YYYY');

    const toBoolean = (value) => $.trim(value).toLowerCase() === 'true';

    const getAttribute = ($element, name) => {
        const value = $element.attr(name);
        return typeof value === 'undefined' || value === '' ? undefined : value;
    };

    const parseLocalizedDate = (value, format) => {
        const trimmed = $.trim(value);
        return getLang() === 'vi' ? trimmed : moment(trimmed, format).format(format);
    };

    const resolveFunction = (value) => {
        const name = $.trim(value);
        const fn = window[name];
        return typeof fn === 'function' ? fn : undefined;
    };

    const buildLocale = (dateFormat) => {
        if (getLang() === 'vi') {
            return {
                format: dateFormat,
                separator: ' - ',
                applyLabel: 'Đồng ý',
                cancelLabel: 'Hủy',
                fromLabel: 'Từ',
                toLabel: 'đến',
                customRangeLabel: 'Tùy chọn',
                daysOfWeek: ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'],
                monthNames: [
                    'Tháng 1',
                    'Tháng 2',
                    'Tháng 3',
                    'Tháng 4',
                    'Tháng 5',
                    'Tháng 6',
                    'Tháng 7',
                    'Tháng 8',
                    'Tháng 9',
                    'Tháng 10',
                    'Tháng 11',
                    'Tháng 12'
                ]
            };
        }

        return {
            format: dateFormat,
            separator: ' - '
        };
    };

    const buildDisplayFormat = ($element) => {
        let displayFormat = getFormat();
        if ($element.attr('data-timepicker') === 'true') {
            displayFormat += $element.attr('data-timepicker24hour') === 'true' ? ' HH:mm' : ' hh:mm A';
            if ($element.attr('data-timepickerseconds') === 'true') {
                displayFormat += ':ss';
            }
        }
        return displayFormat;
    };

    const getHiddenField = ($element) => {
        const hiddenFieldId = $element.attr('data-hdf');
        return hiddenFieldId ? $("#" + hiddenFieldId) : $();
    };

    const triggerInlineChange = ($element) => {
        const inlineHandler = getAttribute($element, 'data-onchange');
        if (inlineHandler) {
            const func = new Function(inlineHandler);
            func();
        }
    };

    const updatePickerInstance = ($element, parsedDate, format) => {
        const pickerInstance = $element.data('daterangepicker');
        if (!pickerInstance) {
            return;
        }

        pickerInstance.setStartDate(parsedDate);
        pickerInstance.setEndDate(parsedDate);
        $element.attr('data-startdate', parsedDate.format(format));
        $element.attr('data-enddate', parsedDate.format(format));
    };

    const updateHiddenFieldValue = ($element, parsedDate, format) => {
        const hiddenField = getHiddenField($element);
        if (!hiddenField.length) {
            return;
        }

        if (parsedDate) {
            const formatted = parsedDate.format(format);
            if ($element.attr('data-singledatepicker') === 'true') {
                hiddenField.val(formatted);
            } else {
                hiddenField.val(`${formatted} - ${formatted}`);
            }
        } else {
            hiddenField.val('');
        }
    };

    const clearInputState = ($element) => {
        $element.val('');
        updateHiddenFieldValue($element, null);
        const pickerInstance = $element.data('daterangepicker');
        if (pickerInstance) {
            $element.attr('data-startdate', '');
            $element.attr('data-enddate', '');
        }
    };

    const applyParsedDate = ($element, parsedDate, format) => {
        updatePickerInstance($element, parsedDate, format);
        updateHiddenFieldValue($element, parsedDate, format);
        //triggerInlineChange($element);
    };

    const createRanges = () => {
        if (getLang() === 'vi') {
            return {
                'Hôm nay': [moment(), moment()],
                'Hôm qua': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                '7 ngày trước': [moment().subtract(6, 'days'), moment()],
                '30 ngày trước': [moment().subtract(29, 'days'), moment()],
                'Tháng này': [moment().startOf('month'), moment().endOf('month')],
                'Tháng trước': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
            };
        }

        return {
            'To day': [moment(), moment()],
            Yesterday: [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
            '7 days ago': [moment().subtract(6, 'days'), moment()],
            '30 days ago': [moment().subtract(29, 'days'), moment()],
            'This month': [moment().startOf('month'), moment().endOf('month')],
            'Last month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
        };
    };

    const createMaskFormatter = (mask) => {
        const formatValue = (value) => {
            let formatted = '';
            let inputIndex = 0;

            for (let i = 0; i < mask.length && inputIndex < value.length; i++) {
                if (mask[i] === '_') {
                    let char = value[inputIndex];
                    while (inputIndex < value.length && !DIGIT_REGEX.test(char)) {
                        inputIndex++;
                        char = value[inputIndex];
                    }

                    if (inputIndex < value.length && DIGIT_REGEX.test(char)) {
                        formatted += char;
                        inputIndex++;
                    } else {
                        formatted += '_';
                    }
                } else {
                    formatted += mask[i];
                    if (inputIndex < value.length && value[inputIndex] === mask[i]) {
                        inputIndex++;
                    }
                }
            }

            while (formatted.length < mask.length) {
                formatted += '_';
            }

            return formatted;
        };

        const countDigitsBeforeCursor = (value, cursorPosition) => {
            let count = 0;
            for (let i = 0; i < cursorPosition && i < value.length; i++) {
                if (DIGIT_REGEX.test(value[i])) {
                    count++;
                }
            }
            return count;
        };

        const findCursorPosition = (formattedValue, targetDigits) => {
            if (targetDigits <= 0) {
                for (let i = 0; i < formattedValue.length; i++) {
                    if (formattedValue[i] === '_') {
                        return i;
                    }
                }
            }

            let currentDigits = 0;
            for (let i = 0; i < formattedValue.length; i++) {
                const char = formattedValue[i];
                if (char !== '_' && DIGIT_REGEX.test(char)) {
                    currentDigits++;
                    if (currentDigits >= targetDigits) {
                        return i + 1;
                    }
                } else if (char === '_' && currentDigits >= targetDigits) {
                    return i;
                }
            }

            return formattedValue.length;
        };

        const skipNonEditable = (formattedValue, position) => {
            let newPosition = position;
            while (
                newPosition < formattedValue.length &&
                formattedValue[newPosition] !== '_' &&
                !DIGIT_REGEX.test(formattedValue[newPosition])
            ) {
                newPosition++;
            }
            return newPosition;
        };

        return {
            formatValue,
            calculateCursor: (rawValue, formattedValue, cursorPosition) => {
                const digitsBeforeCursor = countDigitsBeforeCursor(rawValue, cursorPosition);
                const newPosition = findCursorPosition(formattedValue, digitsBeforeCursor);
                return skipNonEditable(formattedValue, newPosition);
            }
        };
    };

    const createMaskNavigator = (mask) => {
        const findPreviousEditable = (position) => {
            let newPos = position - 1;
            while (newPos >= 0 && mask[newPos] !== '_') {
                newPos--;
            }
            return newPos;
        };

        const findNextEditable = (position) => {
            let newPos = position;
            while (newPos < mask.length && mask[newPos] !== '_') {
                newPos++;
            }
            return newPos;
        };

        return {
            findPreviousEditable,
            findNextEditable
        };
    };

    const setCursor = (input, position) => {
        setTimeout(() => {
            input.setSelectionRange(position, position);
        }, 0);
    };

    const applyInputMask = (element, format) => {
        if (!element || element.length === 0) {
            return;
        }

        const mask = createInputMask(format);
        const { formatValue, calculateCursor } = createMaskFormatter(mask);
        const { findPreviousEditable, findNextEditable } = createMaskNavigator(mask);

        const resetToMask = (input) => {
            $(input).val(mask);
            setCursor(input, 0);
        };

        element.on('focus', function () {
            const currentValue = $(this).val();
            if (currentValue === '' || currentValue === mask) {
                resetToMask(this);
            }
        });

        element.on('input', function () {
            const input = this;
            const $input = $(input);
            const rawValue = $input.val();

            if (rawValue === '') {
                resetToMask(input);
                return;
            }

            const cursorPosition = input.selectionStart;
            const formatted = formatValue(rawValue);
            $input.val(formatted);

            const newCursorPosition = calculateCursor(rawValue, formatted, cursorPosition);
            setCursor(input, newCursorPosition);

            if (!formatted.includes('_')) {
                const parsedDate = parseInputDate(formatted, format);
                if (parsedDate) {
                    applyParsedDate($input, parsedDate, format);
                }
            }
        });

        element.on('keydown', function (e) {
            const input = this;
            const $input = $(input);
            const value = $input.val();
            const selectionStart = input.selectionStart;
            const selectionEnd = input.selectionEnd;

            const allowNavigationKeys = ['Tab', 'ArrowLeft', 'ArrowRight', 'Home', 'End'];
            if (
                allowNavigationKeys.includes(e.key) ||
                e.key === 'Shift' ||
                e.key === 'Control' ||
                e.key === 'Alt' ||
                e.metaKey ||
                (e.ctrlKey && ['a', 'c', 'v', 'x', 'z'].includes(e.key.toLowerCase()))
            ) {
                if (e.key === 'Backspace' || e.key === 'Delete') {
                    e.preventDefault();
                }
                return;
            }

            if (e.key === 'Backspace' || e.key === 'Delete') {
                e.preventDefault();

                if (selectionStart !== selectionEnd) {
                    const chars = value.split('');
                    for (let i = selectionStart; i < selectionEnd; i++) {
                        if (mask[i] === '_') {
                            chars[i] = '_';
                        }
                    }
                    $input.val(chars.join(''));
                    setCursor(input, selectionStart);
                    return;
                }

                if (e.key === 'Backspace' && selectionStart > 0) {
                    const target = findPreviousEditable(selectionStart);
                    if (target >= 0) {
                        const chars = value.split('');
                        chars[target] = '_';
                        $input.val(chars.join(''));
                        setCursor(input, target);
                    }
                    return;
                }

                if (e.key === 'Delete' && selectionStart < value.length) {
                    const target = findNextEditable(selectionStart);
                    if (target < value.length) {
                        const chars = value.split('');
                        chars[target] = '_';
                        $input.val(chars.join(''));
                        setCursor(input, selectionStart);
                    }
                }
                return;
            }

            if (!DIGIT_REGEX.test(e.key)) {
                e.preventDefault();
                return;
            }

            e.preventDefault();

            const chars = value.split('');
            if (selectionStart !== selectionEnd) {
                let replacePosition = selectionStart;
                while (replacePosition < selectionEnd && mask[replacePosition] !== '_') {
                    replacePosition++;
                }

                if (replacePosition < selectionEnd) {
                    chars[replacePosition] = e.key;
                    for (let i = replacePosition + 1; i < selectionEnd; i++) {
                        if (mask[i] === '_') {
                            chars[i] = '_';
                        }
                    }
                    $input.val(chars.join(''));
                    const nextPosition = findNextEditable(replacePosition + 1);
                    setCursor(input, nextPosition);
                }
                return;
            }

            const insertionPoint = findNextEditable(selectionStart);
            if (insertionPoint >= value.length) {
                return;
            }

            chars[insertionPoint] = e.key;
            $input.val(chars.join(''));
            const nextCursor = findNextEditable(insertionPoint + 1);
            setCursor(input, nextCursor);
        });

        element.on('blur', function () {
            const input = this;
            const $input = $(input);
            const value = $input.val();

            if (value === mask || value.includes('_')) {
                clearInputState($input);
                return;
            }

            const parsedDate = parseInputDate(value, format);
            if (parsedDate) {
                applyParsedDate($input, parsedDate, format);
            } else {
                clearInputState($input);
            }
        });
    };

    const createInputMask = (format) => format.replace(/[YMDHms]/g, '_');

    const parseInputDate = (value, format) => {
        if (!value || value.includes('_')) {
            return null;
        }
        const parsed = moment(value, format, true);
        return parsed.isValid() ? parsed : null;
    };

    const renderElement = (element) => {
        const $element = $(element);
        const options = {};

        const dateFormatAttr = getAttribute($element, 'data-dateFormat');
        if (dateFormatAttr) {
            options.dateFormat = $.trim(dateFormatAttr);
        }

        const dateFormat = dateFormatAttr ? $.trim(dateFormatAttr) : 'DD/MM/YYYY';

        const localizedDateAttributes = [
            { attr: 'data-startDate', option: 'startDate' },
            { attr: 'data-endDate', option: 'endDate' },
            { attr: 'data-minDate', option: 'minDate' },
            { attr: 'data-maxDate', option: 'maxDate' }
        ];

        localizedDateAttributes.forEach(({ attr, option }) => {
            const value = getAttribute($element, attr);
            if (value) {
                options[option] = parseLocalizedDate(value, dateFormat);
            }
        });

        const booleanAttributes = [
            'showDropdowns',
            'showWeekNumbers',
            'showISOWeekNumbers',
            'timePicker',
            'timePicker24Hour',
            'timePickerSeconds',
            'showCustomRangeLabel',
            'alwaysShowCalendars',
            'singleDatePicker',
            'autoApply',
            'linkedCalendars',
            'autoUpdateInput',
            'allowNullDate'
        ];

        booleanAttributes.forEach((option) => {
            const value = getAttribute($element, `data-${option}`);
            if (value) {
                options[option] = toBoolean(value);
            }
        });

        const stringAttributes = [
            'minYear',
            'maxYear',
            'timePickerIncrement',
            'opens',
            'drops',
            'buttonClasses',
            'applyButtonClasses',
            'cancelButtonClasses'
        ];

        stringAttributes.forEach((option) => {
            const value = getAttribute($element, `data-${option}`);
            if (value) {
                options[option] = $.trim(value);
            }
        });

        const predefinedDateRanges = getAttribute($element, 'data-predefinedDateRanges');
        if (predefinedDateRanges && toBoolean(predefinedDateRanges)) {
            options.ranges = createRanges();
        }

        const functionAttributes = [
            { attr: 'data-isInvalidDate', option: 'isInvalidDate' },
            { attr: 'data-isCustomDate', option: 'isCustomDate' },
            { attr: 'data-onClose', option: 'onClose' },
            { attr: 'data-onOpen', option: 'onOpen' },
            { attr: 'data-onReady', option: 'onReady' }
        ];

        functionAttributes.forEach(({ attr, option }) => {
            const value = getAttribute($element, attr);
            const handler = value ? resolveFunction(value) : undefined;
            if (handler) {
                options[option] = handler;
            }
        });

        const onChangeAttr = getAttribute($element, 'data-onChange');
        if (onChangeAttr) {
            const handler = resolveFunction(onChangeAttr);
            if (handler) {
                options.onChange = handler;
            } else if (typeof onChangeAttr === 'string') {
                options.onChange = eval(onChangeAttr); // eslint-disable-line no-eval
            }
        }

        options.locale = buildLocale(dateFormat);

        if (options.singleDatePicker === true) {
            let inputFormat = dateFormat;
            if (options.timePicker === true) {
                inputFormat += options.timePicker24Hour === true ? ' HH:mm' : ' hh:mm A';
                if (options.timePickerSeconds === true) {
                    inputFormat += ':ss';
                }
            }
            applyInputMask($element, inputFormat);
        }

        const picker = $element
            .daterangepicker(options)
            .on('apply.daterangepicker', function (ev, pickerInstance) {
                const displayFormat = buildDisplayFormat($element);
                $element.addClass('ignore');

                const startFormatted = pickerInstance.startDate.format(displayFormat);
                const endFormatted = pickerInstance.endDate.format(displayFormat);
                const value = `${startFormatted} - ${endFormatted}`;

                const hiddenField = getHiddenField($element);
                if (hiddenField.length) {
                    hiddenField.val(value);
                    hiddenField.trigger('change');
                }

                if (
                    $element.attr('data-singledatepicker') === 'false' ||
                    $element.attr('data-predefineddateranges') === 'true'
                ) {
                    $element.val(value);
                } else {
                    $element.val(startFormatted);
                }

                $element.attr('data-startdate', startFormatted);
                $element.attr('data-enddate', endFormatted);
                triggerInlineChange($element);
            })
            .on('cancel.daterangepicker', function () {
                if ($element.hasClass('valid-success')) {
                    $element.addClass('ignore');
                } else {
                    $element.removeClass('ignore');
                }

                const hiddenField = getHiddenField($element);
                if (hiddenField.length) {
                    hiddenField.val('');
                    hiddenField.trigger('change');
                }

                $element.val('').trigger('change');
            })
            .on('change', function (ev) {
                const $target = $(ev.target);
                const value = $target.val();
                const hiddenField = getHiddenField($target);

                if ($target.hasClass('applying-mask')) {
                    return;
                }

                const displayFormat = buildDisplayFormat($target);

                if ($target.attr('data-singledatepicker') === 'true') {
                    const parsedDate = parseInputDate(value, displayFormat);
                    if (parsedDate) {
                        const pickerInstance = $target.data('daterangepicker');
                        if (pickerInstance) {
                            pickerInstance.setStartDate(parsedDate);
                            pickerInstance.setEndDate(parsedDate);
                            $target.attr('data-startdate', parsedDate.format(displayFormat));
                            $target.attr('data-enddate', parsedDate.format(displayFormat));
                        }

                        if (hiddenField.length) {
                            hiddenField.val(parsedDate.format(displayFormat));
                            hiddenField.trigger('change');
                        }
                    } else if (value === '' && hiddenField.length) {
                        hiddenField.val('');
                        hiddenField.trigger('change');
                    }
                    return;
                }

                const dates = value.split(' - ');
                const start = moment(dates[0], displayFormat, true);
                const end = dates.length > 1 ? moment(dates[1], displayFormat, true) : start;

                if (start.isValid() && end.isValid()) {
                    const pickerInstance = $target.data('daterangepicker');
                    if (pickerInstance) {
                        pickerInstance.setStartDate(start);
                        pickerInstance.setEndDate(end);
                        $target.attr('data-startdate', start.format(displayFormat));
                        $target.attr('data-enddate', end.format(displayFormat));
                    }

                    if (hiddenField.length) {
                        hiddenField.val(value);
                        hiddenField.trigger('change');
                    }

                    $target.trigger('apply.daterangepicker', {
                        startDate: start,
                        endDate: end
                    });
                } else {
                    if (hiddenField.length) {
                        hiddenField.val('');
                        hiddenField.trigger('change');
                    }
                    $target.val('');
                }
            });

        setTimeout(() => {
            const pickerData = $element.data('daterangepicker');
            $element.removeClass('text-white');
            let hiddenField = getHiddenField($element);

            if ($element.hasClass('ignore')) {
                if (hiddenField.val() === '-') {
                    $element.val('');
                }
                return;
            }

            if (pickerData && pickerData.startDate && pickerData.endDate) {
                if ($element.attr('data-predefineddateranges') === 'true') {
                    if (hiddenField.val() === '-') {
                        $element.val('');
                    }
                    return;
                }

                $element.val('');
                if (hiddenField.length) {
                    hiddenField.val('');
                    hiddenField.trigger('change');
                }

                if ($element.attr('data-allownulldate') === 'true') {
                    return;
                }

                const displayFormat = buildDisplayFormat($element);

                if (
                    $element.attr('data-singledatepicker') === 'true' &&
                    $element.attr('data-predefineddateranges') !== 'true'
                ) {
                    $element.val(pickerData.startDate.format(displayFormat));
                } else {
                    const displayValue = `${pickerData.startDate.format(displayFormat)} - ${pickerData.endDate.format(displayFormat)}`;
                    $element.val(displayValue);
                }

                hiddenField = getHiddenField($element);
                if (hiddenField.length && hiddenField.val() === '') {
                    const hiddenValue = `${pickerData.startDate.format(displayFormat)} - ${pickerData.endDate.format(displayFormat)}`;
                    hiddenField.val(hiddenValue);
                    hiddenField.trigger('change');
                }
            }
        }, 50);

        return picker;
    };

    const forEachElement = () => {
        $('[data-control="extra-datetime"]').each(function () {
            renderElement($(this));
        });
    };

    return {
        get Lang() {
            return getLang();
        },
        get Format() {
            return getFormat();
        },
        createInputMask,
        applyInputMask,
        parseInputDate,
        ForElement: forEachElement,
        RenderElement: renderElement,
        onValueUpdate: (selectedDates, dateStr, instance) => {
            const $input = $(instance.input);
            if (!$input.length) {
                return;
            }

            const hiddenField = getHiddenField($input);
            if (!hiddenField.length) {
                return;
            }

            if (!selectedDates || !selectedDates.length) {
                hiddenField.val('');
                hiddenField.trigger('change');
                return;
            }

            hiddenField.val(new Date(selectedDates[0]).toLocaleString('en-US'));
            hiddenField.trigger('change');
        },
        onChange: (selectedDates, dateStr, instance) => {
            const $input = $(instance.input);
            if (!$input.length) {
                return;
            }

            const hiddenField = getHiddenField($input);
            if (!hiddenField.length) {
                return;
            }

            if (!selectedDates || !selectedDates.length) {
                hiddenField.val('');
                hiddenField.trigger('change');
            }
        },
        Init: () => {
            ExtraDatetimeJs.ForElement();
        }
    };
})();

$(function () {
    window.ExtraDateTimeChange = function (postBackEvent) {
        if (window.ExtraDateChangeTimeout !== undefined) {
            clearTimeout(window.ExtraDateChangeTimeout);
        }

        window.ExtraDateChangeTimeout = setTimeout(() => {
            eval(postBackEvent); // eslint-disable-line no-eval
        }, 500);
    };

    ExtraDatetimeJs.Init();
    if (typeof Sys !== 'undefined' && typeof Sys.WebForms !== 'undefined') {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ExtraDatetimeJs.Init);
    }
});