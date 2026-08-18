const ExtraGridviewJs = {
    resourceText: {
        vi: {
            SELECT: "Chọn",
            SHOW_COLUMNS: "Hiển thị cột",
            SHOW_ALL: "Hiển thị tất cả",
        },
        en: {
            SELECT: "Select",
            SHOW_COLUMNS: "Show columns",
            SHOW_ALL: "Show all",
        }
    },

    // Store individual table states
    tableStates: new Map(),

    isValidGUID: str => /^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$/.test(str || ""),

    // Get or create table state
    getTableState(tableId) {
        if (!this.tableStates.has(tableId)) {
            this.tableStates.set(tableId, {
                adjustedManually: false,
                initialized: false
            });
        }
        return this.tableStates.get(tableId);
    },

    async setVisibleColumns($table, columns) {
        $table.find("thead tr th:not([id*=clone])").each((index, el) => {
            $(el).attr("data-priority", columns.includes(index.toString()) ? 1 : 0);
        });
    },

    async renderResponsiveTables() {
        $('div.table-extra:not([id*=clone])').each(async function () {
            const $container = $(this);
            await ExtraGridviewJs.initSingleTable($container);
        });
    },

    async initSingleTable($container) {
        const containerId = $container.attr('id') || `table-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        if (!$container.attr('id')) {
            $container.attr('id', containerId);
        }

        const tableState = this.getTableState(containerId);
        if (tableState.initialized) return;

        const $table = $container.find('table');
        if (!$table.length) return;

        const columnsAttr = $container.attr("data-VisibledColumns");
        const columns = columnsAttr ? columnsAttr.split(',') : [];
        if (columns.length) await this.setVisibleColumns($container, columns);

        const lang = $('html').attr('lang') || 'vi';
        const opts = {
            pattern: $container.attr("data-pattern"),
            stickyTableHeader: JSON.parse($container.attr("data-stickytableheader") || 'false'),
            fixedNavbar: $container.attr("data-fixedNavbar") || '',
            addDisplayAllBtn: JSON.parse($container.attr("data-addDisplayAllBtn") || 'false'),
            addFocusBtn: JSON.parse($container.attr("data-addFocusBtn") || 'false'),
            focusBtnIcon: $container.attr("data-focusBtnIcon"),
            i18n: {
                focus: this.resourceText[lang].SHOW_ALL,
                display: this.resourceText[lang].SHOW_COLUMNS,
                displayAll: this.resourceText[lang].SHOW_ALL
            }
        };

        $container.responsiveTable(opts);

        if (JSON.parse($container.attr("data-enableselectcolumn") || 'false')) {
            const tableId = $container.find('.extra-gridview:not([id*=clone])').attr('id');
            const hdfId = $container.attr('data-hdfvalue');
            this.bindSelectColumnEvents(tableId, hdfId);
        }

        // Apply individual height adjustment
        const skuHeight = parseFloat($container.attr("data-adjust-height")) || 0;
        this.adjustSingleTableViewportHeight($container[0], skuHeight);

        tableState.initialized = true;
    },

    bindSelectColumnEvents(tableId, hdfValueId) {
        const $table = $(`#${tableId}`);

        // Remove existing event handlers to prevent duplicates
        $table.find('.inp-cbx').off('change.gridview');

        $table.find('.inp-cbx').not('.inp-cbx-all').on('change.gridview', () => {
            this.updateCheckAllState(tableId);
            this.collectSelectedData(tableId, hdfValueId);
        });

        $table.find('.inp-cbx-all').on('change.gridview', (e) => {
            const isChecked = $(e.currentTarget).is(':checked');
            $table.find('.inp-cbx').not('.inp-cbx-all').prop('checked', isChecked);
            this.collectSelectedData(tableId, hdfValueId);
        });

        this.collectSelectedData(tableId, hdfValueId);
    },

    updateCheckAllState(tableId) {
        const $table = $(`#${tableId}`);
        const $checkboxes = $table.find('.inp-cbx').not('.inp-cbx-all');
        const allChecked = $checkboxes.length === $checkboxes.filter(':checked').length;
        $table.find('.inp-cbx-all').prop('checked', allChecked);
    },

    collectSelectedData(tableId, hdfValueId) {
        const selected = [];
        $(`#${tableId} .inp-cbx:checked`).not('.inp-cbx-all').each((_, item) => {
            const $item = $(item);
            const id = $item.val();
            const name = $item.data('name');
            if (this.isValidGUID(id)) {
                selected.push({ Id: id, Name: name });
            }
        });
        $(`#${hdfValueId}`).val(JSON.stringify(selected));
    },

    removeSelectedItem(el) {
        const id = $(el).data('id');
        if (this.isValidGUID(id)) {
            $(`#cbx-${id}`).prop('checked', false).trigger('change');
        }
        $(el).closest('.searchTag').remove();
    },

    updateSingleTable($container) {
        if ($container && $container.length) {
            $container.responsiveTable('update');
        }
    },

    updateResponsiveTables() {
        $('.table-extra:not([id*=clone])').each(function () {
            ExtraGridviewJs.updateSingleTable($(this));
        });
    },

    adjustSingleTableViewportHeight(tableElement, diffHeight = 0) {
        if (!tableElement) return;
        const $table = $(tableElement);
        const $modal = $table.closest('.modal');
        if ($modal.length > 0 && !$modal.hasClass('show')) {
            // Modal chưa mở => chờ sự kiện shown rồi mới tính
            $modal.one('shown.bs.modal', () => {
                this.adjustSingleTableViewportHeight(tableElement, diffHeight);
            });
            return;
        }

        const tableId = $table.attr('id');
        const tableState = this.getTableState(tableId);

        // Skip if manually adjusted
        if (tableState.adjustedManually) return;

        const topOffset = tableElement.getBoundingClientRect().top;
        const footerHeight = $('.footer').outerHeight() || 0;
        // Get paging height specific to this table
        const $tablePaging = $table.siblings('.table-paging').first();
        const pagingHeight = $tablePaging.length ? $tablePaging.outerHeight() : 0;
        // Get card header height specific to this table's container
        const $cardHeader = $table.closest('.card').find('.card-header').first();
        const cardHeaderHeight = $cardHeader.length ? $cardHeader.outerHeight() : 0;
        if ($(window).width() > 768) {
            let deduction = topOffset + footerHeight + pagingHeight + (diffHeight || 0);
            let adjustPadding = cardHeaderHeight > 200 ? 40 : 65;
            const newHeight = `calc(100vh - ${deduction}px - ${adjustPadding}px)`;

            // Apply styles only to this specific table
            $table.css({
                'max-height': newHeight,
                'overflow-y': 'scroll',
                'min-height': newHeight
            });

            // Apply sticky header class only to this table's header
            $table.find('.sticky-table-header').addClass('table-viewport');
        }
    },

    adjustViewportHeight(table, diffHeight = 0) {
        // Backward compatibility - delegate to single table method
        this.adjustSingleTableViewportHeight(table, diffHeight);
    },

    moveDropDownToolbar() {
        setTimeout(() => {
            $('.dropdown-btn-group').each((_, btn) => {
                const $btn = $(btn);
                const $tableWrapper = $btn.closest('.table-rep-plugin');
                const $paging = $tableWrapper.next().find('.table-paging');
                const $customContainer = $paging.find('.table-custom-show-column');
                if ($customContainer.length && !$btn.parent().is($customContainer)) {
                    $btn.appendTo($customContainer);
                }
            });
        }, 200);
    },

    async initTable($table) {
        // Delegate to the new single table initialization
        await this.initSingleTable($table);
    },

    // Method to manually mark a table as adjusted (prevents auto-adjustment)
    markTableAsManuallyAdjusted(tableId) {
        const tableState = this.getTableState(tableId);
        tableState.adjustedManually = true;
    },

    // Method to reset manual adjustment flag
    resetManualAdjustment(tableId) {
        const tableState = this.getTableState(tableId);
        tableState.adjustedManually = false;
    },

    // Method to reinitialize a specific table (useful for postback scenarios)
    async reinitializeTable(tableId) {
        const $container = $(`#${tableId}`);
        if ($container.length) {
            const tableState = this.getTableState(tableId);
            tableState.initialized = false;

            // Clean up existing event handlers
            $container.find('.inp-cbx').off('change.gridview');

            await this.initSingleTable($container);
        }
    },

    async init() {
        // Initialize all tables individually
        const initPromises = [];
        $('.table-extra:not([id*=clone])').each(function () {
            const $table = $(this);
            initPromises.push(ExtraGridviewJs.initSingleTable($table));
        });

        await Promise.all(initPromises);

        this.moveDropDownToolbar();

        $(".btn-toolbar [type=button]").addClass("btn-sm");
        $(".btn-toolbar [data-toggle=dropdown]").attr("data-bs-toggle", "dropdown").addClass("ignore");
    }
};

$(function () {
    ExtraGridviewJs.init();

    if (typeof Sys !== 'undefined' && typeof Sys.WebForms !== 'undefined') {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(() => {
            ExtraGridviewJs.init();
        });
    }

    // Resize handler - only adjust tables that haven't been manually adjusted
    $(window).on('resize', () => {
        $('.table-extra:not([id*=clone])').each(function () {
            const tableId = $(this).attr('id');
            if (tableId) {
                const tableState = ExtraGridviewJs.getTableState(tableId);
                if (!tableState.adjustedManually) {
                    const diffHeight = parseFloat($(this).attr("data-adjust-height")) || 0;
                    ExtraGridviewJs.adjustSingleTableViewportHeight(this, diffHeight);
                }
            }
        });
    });
});