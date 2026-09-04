(function () {
    "use strict";

    function showEmptyState(element, message) {
        if (!element) {
            return;
        }

        element.innerHTML =
            '<div class="d-flex align-items-center justify-content-center text-muted progress-chart-empty">' +
            message +
            '</div>';
    }

    function renderScheduleChart() {
        var element = document.getElementById("progress-schedule-chart");
        var data = window.dashboardProgressScheduleData || [];

        if (!element || typeof ApexCharts === "undefined") {
            return;
        }

        if (data.length === 0) {
            showEmptyState(element, "Không có dữ liệu tiến độ dự án.");
            return;
        }

        var chartHeight = Math.max(330, data.length * 75);
        element.style.height = chartHeight + "px";

        new ApexCharts(element, {
            chart: {
                type: "bar",
                height: chartHeight,
                toolbar: { show: false },
                parentHeightOffset: 0
            },
            series: [
                {
                    name: "Thực tế",
                    data: data.map(function (item) {
                        return Number(item.actual) || 0;
                    })
                },
                {
                    name: "Kế hoạch theo thời gian",
                    data: data.map(function (item) {
                        return Number(item.planned) || 0;
                    })
                }
            ],
            colors: ["#556ee6", "#f1b44c"],
            plotOptions: {
                bar: {
                    horizontal: true,
                    barHeight: "65%",
                    borderRadius: 3
                }
            },
            xaxis: {
                min: 0,
                max: 100,
                tickAmount: 5,
                categories: data.map(function (item) {
                    return item.code;
                }),
                labels: {
                    formatter: function (value) {
                        return Math.round(Number(value) || 0) + "%";
                    }
                }
            },
            dataLabels: { enabled: false },
            legend: { position: "top", horizontalAlign: "right" },
            tooltip: {
                shared: true,
                intersect: false,
                x: {
                    formatter: function (value, options) {
                        var item = data[options.dataPointIndex];
                        return item ? item.code + " - " + item.name : value;
                    }
                },
                y: {
                    formatter: function (value) {
                        return Number(value).toFixed(1).replace(".0", "") + "%";
                    }
                }
            }
        }).render();
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

        if (total === 0) {
            showEmptyState(element, "Không có công việc trong kỳ.");
            return;
        }

        new ApexCharts(element, {
            chart: {
                type: "donut",
                height: 330,
                toolbar: { show: false }
            },
            labels: data.labels,
            series: data.values.map(function (value) {
                return Number(value) || 0;
            }),
            colors: ["#34c38f", "#50a5f1", "#74788d", "#f46a6a"],
            legend: { position: "bottom" },
            dataLabels: { enabled: true },
            plotOptions: {
                pie: {
                    donut: {
                        size: "68%",
                        labels: {
                            show: true,
                            total: {
                                show: true,
                                label: "Tổng công việc",
                                formatter: function () {
                                    return total;
                                }
                            }
                        }
                    }
                }
            }
        }).render();
    }

    function renderProjectTaskChart() {
        var element = document.getElementById("progress-project-task-chart");
        var data = window.dashboardProgressProjectTaskData || [];

        if (!element || typeof ApexCharts === "undefined") {
            return;
        }

        if (data.length === 0) {
            showEmptyState(element, "Không có dữ liệu công việc theo dự án.");
            return;
        }

        var chartHeight = Math.max(320, data.length * 70);
        element.style.height = chartHeight + "px";

        new ApexCharts(element, {
            chart: {
                type: "bar",
                height: chartHeight,
                stacked: true,
                toolbar: { show: false },
                parentHeightOffset: 0
            },
            series: [
                {
                    name: "Hoàn thành",
                    data: data.map(function (item) { return item.completed; })
                },
                {
                    name: "Đang thực hiện",
                    data: data.map(function (item) { return item.inProgress; })
                },
                {
                    name: "Chưa bắt đầu",
                    data: data.map(function (item) { return item.notStarted; })
                },
                {
                    name: "Quá hạn",
                    data: data.map(function (item) { return item.overdue; })
                }
            ],
            colors: ["#34c38f", "#50a5f1", "#74788d", "#f46a6a"],
            plotOptions: {
                bar: {
                    horizontal: true,
                    barHeight: "55%",
                    borderRadius: 2
                }
            },
            xaxis: {
                categories: data.map(function (item) { return item.code; }),
                min: 0,
                labels: {
                    formatter: function (value) {
                        return Math.round(Number(value) || 0);
                    }
                }
            },
            dataLabels: {
                enabled: true,
                formatter: function (value) {
                    return Number(value) > 0 ? value : "";
                }
            },
            legend: { position: "top", horizontalAlign: "right" },
            tooltip: {
                shared: false,
                x: {
                    formatter: function (value, options) {
                        var item = data[options.dataPointIndex];
                        return item ? item.code + " - " + item.name : value;
                    }
                }
            }
        }).render();
    }

    document.addEventListener("DOMContentLoaded", function () {
        renderScheduleChart();
        renderTaskStatusChart();
        renderProjectTaskChart();
    });
})();
