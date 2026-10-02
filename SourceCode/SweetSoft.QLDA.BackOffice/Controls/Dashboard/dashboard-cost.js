(function () {
    "use strict";

    var texts = window.dashboardCostTexts || {};

    function showEmptyState(element, message, minimumHeight) {
        if (!element) {
            return;
        }

        element.innerHTML =
            '<div class="d-flex align-items-center justify-content-center text-muted cost-chart-empty"' +
            ' style="min-height:' + (minimumHeight || 300) + 'px">' +
            message +
            '</div>';
    }

    function formatMoney(value) {
        var amount = Number(value) || 0;
        var absoluteAmount = Math.abs(amount);
        var suffix = texts.currencySuffix || "";
        var locale = texts.locale || undefined;

        if (absoluteAmount >= 1000000000) {
            return (amount / 1000000000).toLocaleString(locale, {
                maximumFractionDigits: 2
            }) + (texts.billionSuffix || "");
        }

        if (absoluteAmount >= 1000000) {
            return (amount / 1000000).toLocaleString(locale, {
                maximumFractionDigits: 2
            }) + (texts.millionSuffix || "");
        }

        return amount.toLocaleString(locale, {
            maximumFractionDigits: 0
        }) + suffix;
    }

    function formatCount(value, unit) {
        var count = Number(value) || 0;
        return count.toLocaleString(texts.locale || undefined)
            + (unit ? " " + unit : "");
    }

    function escapeHtml(value) {
        return String(value || "").replace(/[&<>"']/g, function (character) {
            return {
                "&": "&amp;",
                "<": "&lt;",
                ">": "&gt;",
                '"': "&quot;",
                "'": "&#39;"
            }[character];
        });
    }

    function renderProjectComparisonChart() {
        var element = document.getElementById("cost-project-comparison-chart");
        var data = window.dashboardCostProjectData || {};
        var projects = Array.isArray(data.projects) ? data.projects : [];
        var list = document.getElementById("costProjectVarianceList");
        var rows = list ? Array.prototype.slice.call(
            list.querySelectorAll("[data-cost-project-row]")
        ) : [];
        var search = document.getElementById("costProjectVarianceSearch");
        var clearFilter = document.getElementById("costProjectVarianceClearFilter");
        var emptyRow = document.getElementById("costProjectVarianceEmpty");
        var countLabel = document.getElementById("costProjectVarianceCount");
        var activeFilter = "";
        var searchQuery = "";

        function getProjectVariance(project) {
            var variance = project.variance;
            if (variance === undefined) { variance = project.CostVariance; }
            if (variance !== undefined && variance !== null) {
                return Number(variance);
            }

            // Retain compatibility with the older project-chart payload.
            variance = project.expectedProfit;
            if (variance === undefined) { variance = project.GrossProfit; }
            if (variance !== undefined && variance !== null) {
                return Number(variance);
            }

            var contractValue = project.contractValue;
            if (contractValue === undefined) { contractValue = project.ContractValue; }
            var actualCost = project.actualCost;
            if (actualCost === undefined) { actualCost = project.ActualCost; }
            var hasContract = project.hasContractValue;
            if (hasContract === undefined) { hasContract = project.HasContractValue; }
            if (hasContract && contractValue !== undefined && contractValue !== null) {
                return Number(contractValue) - (Number(actualCost) || 0);
            }
            return null;
        }

        function getProjectCostStatus(project, variance) {
            var status = project.status || project.CostComparisonStatus;
            if (status === "over" || status === "under" || status === "equal") {
                return status;
            }
            return variance < 0 ? "over" : variance > 0 ? "under" : "equal";
        }

        var statusGroups = [
            { key: "over", label: texts.projectCostOver || "", color: "#ef6b72" },
            { key: "under", label: texts.projectCostUnder || "", color: "#34a77a" },
            { key: "equal", label: texts.projectCostEqual || "", color: "#8492a6" }
        ].map(function (group) {
            var matchingProjects = projects.filter(function (project) {
                var variance = getProjectVariance(project);
                return variance !== null
                    && getProjectCostStatus(project, variance) === group.key;
            });
            var totalDeviation = matchingProjects.reduce(function (total, project) {
                return total + (getProjectVariance(project) || 0);
            }, 0);
            return {
                key: group.key,
                label: group.label,
                color: group.color,
                count: matchingProjects.length,
                totalDeviation: totalDeviation,
                averageDeviation: matchingProjects.length
                    ? totalDeviation / matchingProjects.length : 0
            };
        }).filter(function (group) { return group.count > 0; });

        function applyProjectListFilter() {
            var visibleCount = 0;
            rows.forEach(function (row) {
                var matchesStatus = !activeFilter
                    || row.getAttribute("data-cost-project-status") === activeFilter;
                var matchesSearch = !searchQuery || row.textContent
                    .toLocaleLowerCase()
                    .indexOf(searchQuery) >= 0;
                var visible = matchesStatus && matchesSearch;
                row.classList.toggle("d-none", !visible);
                if (visible) { visibleCount += 1; }
            });
            if (emptyRow) { emptyRow.classList.toggle("d-none", visibleCount > 0); }
            if (clearFilter) { clearFilter.classList.toggle("d-none", !activeFilter); }
            if (countLabel) {
                var selectedGroup = statusGroups.filter(function (group) {
                    return group.key === activeFilter;
                })[0];
                countLabel.textContent = (selectedGroup
                    ? selectedGroup.label + " · " : "")
                    + formatCount(visibleCount, texts.projectUnit);
            }
        }

        function selectProjectStatus(status) {
            activeFilter = activeFilter === status ? "" : status;
            applyProjectListFilter();
        }

        if (search) {
            search.addEventListener("input", function () {
                searchQuery = (search.value || "").trim().toLocaleLowerCase();
                applyProjectListFilter();
            });
        }
        if (clearFilter) {
            clearFilter.addEventListener("click", function () {
                activeFilter = "";
                applyProjectListFilter();
            });
        }
        applyProjectListFilter();

        if (!element || typeof ApexCharts === "undefined") { return; }
        if (!statusGroups.length) {
            showEmptyState(element,
                texts.projectCostNoComparableData || texts.budgetComparisonEmpty || "", 280);
            return;
        }

        var values = statusGroups.map(function (group) { return group.count; });
        var colors = statusGroups.map(function (group) { return group.color; });
        var labels = statusGroups.map(function (group) { return group.label; });
        var totalComparedProjects = values.reduce(function (sum, value) {
            return sum + value;
        }, 0);

        function positionProjectDonutLabels(context) {
            DashboardDonut.schedule(element, context, values, colors,
                formatCount(totalComparedProjects), texts.projectUnit || "");
        }

        element.style.cursor = "pointer";
        var chart = new ApexCharts(element, {
            chart: {
                type: "donut",
                height: 340,
                toolbar: { show: false },
                animations: { enabled: false },
                events: {
                    dataPointSelection: function (event, context, config) {
                        var group = statusGroups[config.dataPointIndex];
                        if (group) { selectProjectStatus(group.key); }
                    },
                    legendClick: function (context, seriesIndex) {
                        var group = statusGroups[seriesIndex];
                        if (group) { selectProjectStatus(group.key); }
                    },
                    mounted: positionProjectDonutLabels,
                    updated: positionProjectDonutLabels,
                    resized: positionProjectDonutLabels
                }
            },
            labels: labels,
            series: values,
            colors: colors,
            legend: {
                position: "bottom",
                horizontalAlign: "center",
                onItemClick: { toggleDataSeries: false },
                formatter: function (name) { return name; }
            },
            dataLabels: {
                enabled: true,
                formatter: function (percentage, options) {
                    var group = statusGroups[options.seriesIndex];
                    return group ? formatCount(group.count) : "";
                },
                style: { fontSize: "12px", fontWeight: 800, colors: ["#fff"] },
                dropShadow: { enabled: true, top: 1, left: 0, blur: 2, color: "#1b293e", opacity: .55 }
            },
            tooltip: {
                custom: function (options) {
                    var group = statusGroups[options.dataPointIndex];
                    if (!group) { return ""; }
                    return '<div class="p-2 cost-project-tooltip">'
                        + '<div class="fw-semibold mb-1">' + escapeHtml(group.label)
                        + ' · ' + escapeHtml(formatCount(group.count, texts.projectUnit)) + '</div>'
                        + '<div>' + escapeHtml(texts.projectCostTotalDeviation || "")
                        + ': <strong>' + escapeHtml(formatSignedMoney(group.totalDeviation)) + '</strong></div>'
                        + '<div>' + escapeHtml(texts.projectCostAverageDeviation || "")
                        + ': <strong>' + escapeHtml(formatSignedMoney(group.averageDeviation)) + '</strong></div>'
                        + '</div>';
                }
            },
            plotOptions: {
                pie: {
                    expandOnClick: false,
                    dataLabels: { minAngleToShowLabel: 0 },
                    donut: { size: "66%", labels: { show: false } }
                }
            },
            stroke: { width: 2, colors: ["#fff"] },
            states: { active: { filter: { type: "none" } } }
        });
        chart.render().then(function () {
            positionProjectDonutLabels(chart);
        });
    }

    function formatSignedMoney(value) {
        var amount = Number(value) || 0;
        return (amount > 0 ? "+" : "") + formatMoney(amount);
    }

    function renderPaymentChart() {
        var element = document.getElementById("cost-payment-chart");
        var data = window.dashboardCostPaymentData || {};
        var categories = [
            { key: "paid-on-time", label: texts.paid || "", data: data.paid, color: "#34a77a" },
            { key: "paid-late", label: texts.paidLate || "", data: data.paidLate, color: "#7b61a8" },
            { key: "due-today", label: texts.dueToday || "", data: data.dueToday, color: "#f1b44c" },
            { key: "upcoming", label: texts.upcoming || "", data: data.upcoming, color: "#4d91d8" },
            { key: "overdue", label: texts.overdue || "", data: data.overdue, color: "#ef6b72" },
            { key: "no-date", label: texts.noDueDate || "", data: data.noDueDate, color: "#94a3b8" }
        ].filter(function (item) {
            return Number(item.data && item.data.amount) > 0;
        });
        var values = categories.map(function (item) {
            return Number(item.data.amount) || 0;
        });
        var total = Number(data.totalAmount) || values.reduce(function (sum, value) {
            return sum + value;
        }, 0);
        var labels = categories.map(function (item) { return item.label; });
        var keys = categories.map(function (item) { return item.key; });
        var colors = categories.map(function (item) { return item.color; });

        if (!element || typeof ApexCharts === "undefined") { return; }
        if (total === 0) {
            showEmptyState(element, texts.noContractOrPayment || "", 300);
            return;
        }

        function positionLabels(context) {
            DashboardDonut.schedule(element, context, values, colors,
                formatMoney(total),
                texts.totalPayments || "");
        }
        element.style.cursor = "pointer";
        var chart = new ApexCharts(element, {
            chart: {
                type: "donut",
                height: 300,
                toolbar: { show: false },
                animations: { enabled: false },
                events: {
                    dataPointSelection: function (event, context, config) {
                        var index = config.dataPointIndex;
                        openCostModal("costPaymentProjectsModal", labels[index], {
                            paymentStatus: keys[index]
                        });
                    },
                    mounted: positionLabels,
                    updated: positionLabels,
                    resized: positionLabels
                }
            },
            labels: labels,
            series: values,
            colors: colors,
            legend: {
                position: "bottom",
                formatter: function (name, options) {
                    return name + ": " + formatMoney(values[options.seriesIndex]);
                }
            },
            dataLabels: {
                enabled: true,
                formatter: DashboardDonut.share,
                style: { fontSize: "11px", fontWeight: 800, colors: ["#fff"] },
                dropShadow: { enabled: true, top: 1, left: 0, blur: 2, color: "#1b293e", opacity: .55 }
            },
            tooltip: {
                custom: function (options) {
                    var item = categories[options.dataPointIndex];
                    if (!item) { return ""; }
                    return '<div class="p-2"><div class="fw-semibold">'
                        + escapeHtml(item.label) + "</div><div><strong>"
                        + escapeHtml(formatMoney(item.data.amount)) + '</strong></div><div class="small">'
                        + escapeHtml(formatCount(item.data.count, texts.paymentUnit))
                        + "</div></div>";
                }
            },
            plotOptions: {
                pie: {
                    expandOnClick: false,
                    dataLabels: { minAngleToShowLabel: 0 },
                    donut: { size: "64%", labels: { show: false } }
                }
            },
            states: { active: { filter: { type: "none" } } }
        });
        chart.render().then(function () { positionLabels(chart); });
    }

    function renderApprovalChart() {
        var element = document.getElementById("cost-approval-chart");
        var data = window.dashboardCostApprovalData || {
            approved: { amount: 0, count: 0 },
            pending: { amount: 0, count: 0 },
            rejected: { amount: 0, count: 0 },
            totalAmount: 0
        };
        var categories = [
            { key: "approved", label: texts.approvedCost || "", data: data.approved, color: "#34a77a" },
            { key: "pending", label: texts.pendingCost || "", data: data.pending, color: "#f1b44c" },
            { key: "rejected", label: texts.rejectedCost || "", data: data.rejected, color: "#ef6b72" }
        ].filter(function (item) {
            return Number(item.data && item.data.amount) > 0;
        });
        var values = categories.map(function (item) {
            return Number(item.data.amount) || 0;
        });
        var total = Number(data.totalAmount) || values.reduce(function (sum, value) {
            return sum + value;
        }, 0);
        var labels = categories.map(function (item) { return item.label; });
        var modalIds = {
            approved: "costApprovedItemsModal",
            pending: "costPendingApprovalModal",
            rejected: "costRejectedItemsModal"
        };
        var colors = categories.map(function (item) { return item.color; });

        if (!element || typeof ApexCharts === "undefined") { return; }
        if (total === 0) {
            showEmptyState(element, texts.noCostItems || "", 300);
            return;
        }

        function positionLabels(context) {
            DashboardDonut.schedule(element, context, values, colors,
                formatMoney(total),
                texts.totalCosts || "");
        }
        element.style.cursor = "pointer";
        var chart = new ApexCharts(element, {
            chart: {
                type: "donut",
                height: 300,
                toolbar: { show: false },
                animations: { enabled: false },
                events: {
                    dataPointSelection: function (event, context, config) {
                        var index = config.dataPointIndex;
                        var item = categories[index];
                        if (!item) { return; }
                        openCostModal(modalIds[item.key], labels[index], {
                            costStatus: item.key
                        });
                    },
                    mounted: positionLabels,
                    updated: positionLabels,
                    resized: positionLabels
                }
            },
            labels: labels,
            series: values,
            colors: colors,
            legend: {
                position: "bottom",
                formatter: function (name, options) {
                    return name + ": " + formatMoney(values[options.seriesIndex]);
                }
            },
            dataLabels: {
                enabled: true,
                formatter: DashboardDonut.share,
                style: { fontSize: "11px", fontWeight: 800, colors: ["#fff"] },
                dropShadow: { enabled: true, top: 1, left: 0, blur: 2, color: "#1b293e", opacity: .55 }
            },
            tooltip: {
                custom: function (options) {
                    var item = categories[options.dataPointIndex];
                    if (!item) { return ""; }
                    return '<div class="p-2"><div class="fw-semibold">'
                        + escapeHtml(item.label) + "</div><div><strong>"
                        + escapeHtml(formatMoney(item.data.amount)) + '</strong></div><div class="small">'
                        + escapeHtml(formatCount(item.data.count, texts.costUnit))
                        + "</div></div>";
                }
            },
            plotOptions: {
                pie: {
                    expandOnClick: false,
                    dataLabels: { minAngleToShowLabel: 0 },
                    donut: {
                        size: "64%",
                        labels: { show: false }
                    }
                }
            },
            states: { active: { filter: { type: "none" } } }
        });
        chart.render().then(function () { positionLabels(chart); });
    }
    function openCostModal(id, title, filter) {
        var modal = document.getElementById(id);
        if (!modal || !window.bootstrap || !window.bootstrap.Modal) { return; }
        modal.dataset.costModalTitle = title || "";
        modal.dataset.costProjectId = filter && filter.projectId || "";
        modal.dataset.costPaymentFilter = filter && filter.paymentFilter || "";
        modal.dataset.costPaymentStatus = filter && filter.paymentStatus || "";
        modal.dataset.costStatus = filter && filter.costStatus || "";
        window.bootstrap.Modal.getOrCreateInstance(modal).show();
    }

    function bindDashboardListSearch() {
        var dashboard = document.querySelector(".dashboard-cost");
        if (!dashboard) {
            return;
        }

        dashboard.querySelectorAll("[data-dashboard-list-search]")
            .forEach(function (input) {
                var listId = input.getAttribute(
                    "data-dashboard-list-search");
                var listBody = document.getElementById(listId);
                if (!listBody) {
                    return;
                }

                var rows = Array.prototype.slice.call(
                    listBody.querySelectorAll("[data-search-row]")
                );
                var emptyRow = listBody.querySelector("[data-search-empty]");
                var groups = window.DashboardProjectGroups
                    ? window.DashboardProjectGroups.create(listBody) : null;

                function applySearch() {
                    var query = (input.value || "")
                        .trim()
                        .toLocaleLowerCase();
                    var visibleCount = 0;

                    rows.forEach(function (row) {
                        var matches = !query || row.textContent
                            .toLocaleLowerCase()
                            .indexOf(query) >= 0;
                        var modal = input.closest(".modal");
                        var projectId = modal && modal.dataset.costProjectId;
                        var paymentFilter = modal && modal.dataset.costPaymentFilter;
                        var paymentStatus = modal && modal.dataset.costPaymentStatus;
                        var costStatus = modal && modal.dataset.costStatus;
                        if (projectId && row.getAttribute("data-cost-project-id") !== projectId) {
                            matches = false;
                        }
                        if (paymentFilter && row.getAttribute("data-cost-" + paymentFilter) !== "1") {
                            matches = false;
                        }
                        if (paymentStatus && row.getAttribute("data-cost-payment-status") !== paymentStatus) {
                            matches = false;
                        }
                        if (costStatus && row.getAttribute("data-cost-status") !== costStatus) {
                            matches = false;
                        }
                        row.classList.toggle("d-none", !matches);
                        if (matches) {
                            visibleCount += 1;
                        }
                    });

                    if (emptyRow && rows.length > 0) {
                        emptyRow.classList.toggle(
                            "d-none",
                            visibleCount > 0);
                    }
                    if (groups) {
                        groups.refresh(Boolean(query));
                    }
                }

                input.addEventListener("input", applySearch);
            });

        dashboard.querySelectorAll(".modal").forEach(function (modal) {
            modal.addEventListener("show.bs.modal", function (event) {
                var trigger = event.relatedTarget;
                var title = modal.querySelector(".modal-title");
                var search = modal.querySelector(
                    "[data-dashboard-list-search]");

                modal.dataset.costProjectId = trigger
                    ? "" : (modal.dataset.costProjectId || "");
                modal.dataset.costPaymentFilter = trigger
                    ? (trigger.getAttribute("data-cost-payment-filter") || "")
                    : (modal.dataset.costPaymentFilter || "");
                modal.dataset.costPaymentStatus = trigger
                    ? (trigger.getAttribute("data-cost-payment-status") || "")
                    : (modal.dataset.costPaymentStatus || "");
                modal.dataset.costStatus = trigger
                    ? (trigger.getAttribute("data-cost-status") || "")
                    : (modal.dataset.costStatus || "");
                if (title) {
                    title.textContent = (trigger && trigger.getAttribute("data-cost-modal-title"))
                        || modal.dataset.costModalTitle || title.textContent;
                }

                if (search) {
                    var listBody = document.getElementById(
                        search.getAttribute("data-dashboard-list-search"));
                    if (listBody && listBody.__dashboardProjectGroups) {
                        listBody.__dashboardProjectGroups.reset();
                    }
                    search.value = "";
                    search.dispatchEvent(new Event("input", {
                        bubbles: true
                    }));
                }
            });
            modal.addEventListener("hidden.bs.modal", function () {
                modal.dataset.costProjectId = "";
                modal.dataset.costPaymentFilter = "";
                modal.dataset.costPaymentStatus = "";
                modal.dataset.costStatus = "";
                modal.dataset.costModalTitle = "";
            });
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        renderProjectComparisonChart();
        renderPaymentChart();
        renderApprovalChart();
        bindDashboardListSearch();
    });
})();
