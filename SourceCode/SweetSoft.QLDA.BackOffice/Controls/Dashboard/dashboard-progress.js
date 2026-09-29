(function () {
    "use strict";

    var texts = window.dashboardProgressTexts || {};
    var taskStatusChart = null;

    function showEmptyState(element, message) {
        if (!element) {
            return;
        }

        element.innerHTML =
            '<div class="d-flex align-items-center justify-content-center text-muted progress-chart-empty">' +
            message +
            '</div>';
    }

    function escapeTooltipText(value) {
        return String(value == null ? "" : value).replace(/[&<>"']/g, function (character) {
            return {
                "&": "&amp;",
                "<": "&lt;",
                ">": "&gt;",
                "\"": "&quot;",
                "'": "&#39;"
            }[character];
        });
    }

    function formatProgressPercent(value) {
        return (Number(value) || 0).toFixed(1).replace(/\.0$/, "") + "%";
    }

    function syncSummaryChartCardHeight() {
        var scheduleCard = document.querySelector(".dashboard-progress .progress-schedule-card");
        var taskStatusCard = document.querySelector(".dashboard-progress .progress-task-status-card");

        if (!scheduleCard || !taskStatusCard) {
            return;
        }

        taskStatusCard.style.minHeight = "";

        if (window.matchMedia && !window.matchMedia("(min-width: 1200px)").matches) {
            return;
        }

        var scheduleCardHeight = scheduleCard.getBoundingClientRect().height;

        if (scheduleCardHeight > 0) {
            taskStatusCard.style.minHeight = Math.ceil(scheduleCardHeight) + "px";
        }
    }

    function scheduleSummaryChartCardHeight() {
        window.requestAnimationFrame(function () {
            window.requestAnimationFrame(syncSummaryChartCardHeight);
        });
    }

    var projectProgressChart = null;

    function openProjectTaskList(item) {
        var trigger = document.getElementById("progressProjectTaskTrigger");
        if (!item || !trigger) {
            return;
        }

        trigger.setAttribute("data-task-project-id", item.projectId);
        trigger.setAttribute(
            "data-task-title",
            (texts.totalTasks || "") + ": " + item.projectCode
                + " · " + item.name);
        trigger.click();
    }

    function renderScheduleChart() {
        var element = document.getElementById("progress-schedule-chart");
        var chartWrapper = document.getElementById("progress-schedule-chart-wrapper");
        var data = window.dashboardProgressProjectData || [];
        var chartHeight = 330;
        var visibleProjectCount = 6;
        var viewportWidth = chartWrapper ? chartWrapper.clientWidth : 0;

        if (!element || typeof ApexCharts === "undefined") {
            return;
        }

        if (projectProgressChart) {
            projectProgressChart.destroy();
            projectProgressChart = null;
        }

        if (viewportWidth <= 0) {
            viewportWidth = element.parentElement.clientWidth || 900;
        }

        element.style.height = chartHeight + "px";
        element.style.width = "100%";
        element.innerHTML = "";

        if (data.length === 0) {
            showEmptyState(element, texts.noProjectProgressData || "");
            return;
        }

        var chartWidth = data.length > visibleProjectCount
            ? Math.ceil(viewportWidth * data.length / visibleProjectCount)
            : viewportWidth;
        element.style.width = chartWidth + "px";
        element.style.cursor = "pointer";

        projectProgressChart = new ApexCharts(element, {
            chart: {
                type: "bar",
                height: chartHeight,
                width: chartWidth,
                toolbar: { show: false },
                parentHeightOffset: 0,
                events: {
                    dataPointSelection: function (event, chartContext, config) {
                        var item = data[config.dataPointIndex];
                        openProjectTaskList(item);
                    }
                }
            },
            series: [
                {
                    name: texts.planned || "",
                    data: data.map(function (item) {
                        return item.planned == null ? null : Number(item.planned);
                    })
                },
                {
                    name: texts.actual || "",
                    data: data.map(function (item) {
                        return item.actual == null ? null : Number(item.actual);
                    })
                }
            ],
            colors: ["#c6ceda", "#4a148c"],
            grid: {
                borderColor: "#e9edf3",
                strokeDashArray: 4,
                padding: {
                    top: 20,
                    right: 18,
                    bottom: 8,
                    left: 12
                }
            },
            plotOptions: {
                bar: {
                    horizontal: false,
                    columnWidth: "52%",
                    borderRadius: 4,
                    borderRadiusApplication: "end",
                    dataLabels: { position: "top" }
                }
            },
            xaxis: {
                categories: data.map(function (item) {
                    return item.projectCode || item.name;
                }),
                labels: {
                    rotate: data.length > visibleProjectCount ? -40 : 0,
                    hideOverlappingLabels: true,
                    style: {
                        colors: ["#475467"],
                        fontSize: "12px",
                        fontWeight: 500
                    }
                }
            },
            yaxis: {
                min: 0,
                max: 100,
                tickAmount: 5,
                labels: {
                    formatter: function (value) {
                        return Math.round(Number(value) || 0) + "%";
                    },
                    style: {
                        colors: ["#667085"],
                        fontSize: "11px"
                    }
                },
                title: { text: texts.progressAxis || "" }
            },
            dataLabels: {
                enabled: true,
                offsetY: -18,
                formatter: function (value) {
                    return value == null ? "" : formatProgressPercent(value);
                },
                style: {
                    colors: ["#667085", "#4a148c"],
                    fontSize: "10px",
                    fontWeight: 600
                }
            },
            legend: {
                position: "top",
                horizontalAlign: "right",
                labels: { colors: "#475467" },
                markers: { width: 10, height: 10, radius: 3 },
                itemMargin: { horizontal: 10 }
            },
            tooltip: {
                shared: false,
                intersect: true,
                custom: function (options) {
                    var item = data[options.dataPointIndex];
                    if (!item) {
                        return "";
                    }

                    var taskCountHtml = '<div><strong>'
                        + escapeTooltipText(texts.completedTasks || "")
                        + ":</strong> " + Number(item.completedTaskCount || 0)
                        + "/" + Number(item.taskCount || 0) + "</div>";
                    var overdueCount = Number(item.overdueTaskCount) || 0;
                    var overdueHtml = overdueCount > 0
                        ? '<div class="text-danger"><strong>'
                            + escapeTooltipText(texts.overdueTasks || "")
                            + ":</strong> " + overdueCount + "</div>"
                        : "";
                    var startHtml = item.startDate
                        ? '<div><strong>' + escapeTooltipText(texts.start || "")
                            + ":</strong> " + escapeTooltipText(item.startDate) + "</div>"
                        : "";
                    var expectedHtml = item.expectedEndDate
                        ? '<div><strong>' + escapeTooltipText(texts.expected || "")
                            + ":</strong> " + escapeTooltipText(item.expectedEndDate) + "</div>"
                        : "";
                    var actualCompletionHtml = item.actualCompletionDate
                        ? '<div><strong>' + escapeTooltipText(
                            texts.actualCompletion || "") + ":</strong> "
                            + escapeTooltipText(item.actualCompletionDate) + "</div>"
                        : "";
                    var completionDelayHtml = item.completionDelayText
                        ? '<div class="text-danger">'
                            + escapeTooltipText(item.completionDelayText) + "</div>"
                        : "";
                    var statusHtml = item.status
                        ? '<div class="mb-2"><span class="badge '
                            + escapeTooltipText(item.statusCss
                                || "bg-secondary-subtle text-secondary")
                            + '">' + escapeTooltipText(item.status) + "</span></div>"
                        : "";
                    var statusReasonHtml = item.statusReason
                        ? '<div class="progress-schedule-tooltip__reason">'
                            + escapeTooltipText(item.statusReason) + "</div>"
                        : "";

                    return '<div class="progress-schedule-tooltip">'
                        + '<div class="progress-schedule-tooltip__title">'
                        + escapeTooltipText(item.projectCode) + " · "
                        + escapeTooltipText(item.name) + "</div>"
                        + statusHtml
                        + statusReasonHtml
                        + '<div><strong>' + escapeTooltipText(texts.actual || "")
                        + ":</strong> " + formatProgressPercent(item.actual) + "</div>"
                        + '<div><strong>' + escapeTooltipText(texts.planned || "")
                        + ":</strong> " + formatProgressPercent(item.planned) + "</div>"
                        + taskCountHtml
                        + overdueHtml
                        + startHtml
                        + expectedHtml
                        + actualCompletionHtml
                        + completionDelayHtml
                        + "</div>";
                }
            }
        });
        projectProgressChart.render();
    }

    function renderTaskStatusChart() {
        var element = document.getElementById("progress-task-status-chart");
        var data = window.dashboardProgressTaskStatusData || {
            labels: [],
            values: []
        };
        var total = (data.values || []).reduce(function (sum, value) {
            return sum + (Number(value) || 0);
        }, 0);

        if (!element || typeof ApexCharts === "undefined") {
            return;
        }

        if (taskStatusChart) {
            taskStatusChart.destroy();
            taskStatusChart = null;
        }
        element.innerHTML = "";

        if (total === 0) {
            showEmptyState(element, texts.noTasksInPeriod || "");
            return;
        }

        element.style.cursor = (data.statusCodes || []).length > 0
            ? "pointer"
            : "default";

        taskStatusChart = new ApexCharts(element, {
            chart: {
                type: "donut",
                height: 330,
                toolbar: { show: false },
                events: {
                    dataPointSelection: function (event, chartContext, config) {
                        var statusCode = (data.statusCodes || [])[config.dataPointIndex];
                        var trigger = document.getElementById(
                            "progressTaskStatusCategoryTrigger");

                        if (statusCode !== undefined && trigger) {
                            trigger.setAttribute("data-task-filter", "all");
                            trigger.setAttribute("data-task-status-code", statusCode);
                            trigger.setAttribute(
                                "data-task-title",
                                data.labels[config.dataPointIndex] || texts.totalTasks || "");
                            trigger.click();
                        }
                    }
                }
            },
            plotOptions: {
                pie: {
                    expandOnClick: false,
                    dataLabels: { minAngleToShowLabel: 8 },
                    donut: {
                        size: "68%",
                        labels: {
                            show: true,
                            total: {
                                show: true,
                                label: texts.totalTasks || "",
                                formatter: function () {
                                    return total;
                                }
                            }
                        }
                    }
                }
            },
            states: {
                active: { filter: { type: "none" } }
            },
            labels: data.labels,
            series: data.values.map(function (value) {
                return Number(value) || 0;
            }),
            colors: ["#34c38f", "#50a5f1", "#74788d", "#f46a6a"],
            legend: { position: "bottom" },
            dataLabels: {
                enabled: true,
                formatter: function (percentage, opts) {
                    return Number(data.values[opts.seriesIndex]) || 0;
                },
                style: { fontSize: "12px", fontWeight: 700, colors: ["#18273f"] },
                dropShadow: { enabled: false }
            }
        });
        taskStatusChart.render();
    }

    function bindTaskStatusChartReset() {
        var modal = document.getElementById("progressTaskDetailsModal");
        if (!modal || modal.__taskStatusChartResetBound) {
            return;
        }

        modal.__taskStatusChartResetBound = true;
        modal.addEventListener("hidden.bs.modal", function () {
            renderTaskStatusChart();
        });
    }

    function bindDashboardListSearch() {
        var dashboard = document.querySelector(".dashboard-progress");
        if (!dashboard) {
            return;
        }

        dashboard.querySelectorAll("[data-dashboard-list-search]")
            .forEach(function (input) {
                var listBody = document.getElementById(
                    input.getAttribute("data-dashboard-list-search"));
                if (!listBody) {
                    return;
                }

                var rows = Array.prototype.slice.call(
                    listBody.querySelectorAll("[data-search-row]"));
                var emptyRow = listBody.querySelector("[data-search-empty]");

                function applySearch() {
                    var query = (input.value || "").trim().toLocaleLowerCase();
                    var visibleCount = 0;

                    rows.forEach(function (row) {
                        var matches = !query || row.textContent
                            .toLocaleLowerCase().indexOf(query) >= 0;
                        row.classList.toggle("d-none", !matches);
                        if (matches) {
                            visibleCount += 1;
                        }
                    });

                    if (emptyRow && rows.length > 0) {
                        emptyRow.classList.toggle("d-none", visibleCount > 0);
                    }
                }

                input.addEventListener("input", applySearch);

                var modal = input.closest(".modal");
                if (modal) {
                    modal.addEventListener("show.bs.modal", function () {
                        input.value = "";
                        applySearch();
                    });
                }
            });
    }

    document.addEventListener("DOMContentLoaded", function () {
        var chartResizeTimer;
        window.addEventListener("resize", function () {
            window.clearTimeout(chartResizeTimer);
            chartResizeTimer = window.setTimeout(function () {
                renderScheduleChart();
            }, 150);
        });
        renderScheduleChart();
        renderTaskStatusChart();
        bindTaskStatusChartReset();
        bindDashboardListSearch();
        scheduleSummaryChartCardHeight();
        window.addEventListener("resize", scheduleSummaryChartCardHeight);
    });
})();
