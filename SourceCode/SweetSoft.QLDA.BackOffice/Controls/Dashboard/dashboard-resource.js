(function () {
    "use strict";

    function formatPercent(value) {
        var number = Number(value) || 0;
        return number.toFixed(1).replace(".0", "") + "%";
    }

    function renderTrendChart() {
        var element = document.getElementById("resource-load-trend-chart");
        var data = window.dashboardResourceTrendData || [];

        if (!element || typeof ApexCharts === "undefined") {
            return;
        }

        var actual = [];
        var forecast = [];
        var firstForecastIndex = -1;

        data.forEach(function (item, index) {
            if (item.forecast) {
                if (firstForecastIndex < 0) {
                    firstForecastIndex = index;
                }
                actual.push(null);
                forecast.push(Number(item.utilization) || 0);
            } else {
                actual.push(Number(item.utilization) || 0);
                forecast.push(null);
            }
        });

        if (firstForecastIndex > 0) {
            forecast[firstForecastIndex - 1] = actual[firstForecastIndex - 1];
        }

        new ApexCharts(element, {
            chart: {
                type: "line",
                height: 320,
                toolbar: { show: false },
                zoom: { enabled: false }
            },
            series: [
                { name: "Lịch phân công đã qua", data: actual },
                { name: "Kế hoạch sắp tới", data: forecast }
            ],
            colors: ["#556ee6", "#f1b44c"],
            stroke: {
                curve: "smooth",
                width: [3, 3],
                dashArray: [0, 7]
            },
            markers: { size: 4, strokeWidth: 0 },
            dataLabels: { enabled: false },
            xaxis: {
                categories: data.map(function (item) { return item.label; })
            },
            yaxis: {
                min: 0,
                forceNiceScale: true,
                labels: { formatter: formatPercent },
                title: { text: "Mức sử dụng" }
            },
            annotations: {
                yaxis: [{
                    y: 100,
                    borderColor: "#f46a6a",
                    strokeDashArray: 4,
                    label: {
                        borderColor: "#f46a6a",
                        style: { color: "#fff", background: "#f46a6a" },
                        text: "Ngưỡng 100%"
                    }
                }]
            },
            legend: { position: "top", horizontalAlign: "right" },
            tooltip: {
                shared: true,
                intersect: false,
                x: {
                    formatter: function (value, options) {
                        var item = data[options.dataPointIndex];
                        return item
                            ? item.label + " (" + item.start + "–" + item.end + ")"
                            : value;
                    }
                },
                y: { formatter: formatPercent }
            }
        }).render();
    }

    function appendText(parent, tagName, className, value) {
        var element = document.createElement(tagName);
        if (className) {
            element.className = className;
        }
        element.textContent = value || "";
        parent.appendChild(element);
        return element;
    }

    function findEmployee(employeeId) {
        var employees = window.dashboardResourceDetailData || [];
        var normalizedId = String(employeeId || "").toLowerCase();
        var result = null;

        employees.some(function (employee) {
            if (String(employee.id || "").toLowerCase() === normalizedId) {
                result = employee;
                return true;
            }
            return false;
        });

        return result;
    }

    function findWeek(employee, weekStart) {
        var result = null;
        (employee.weeks || []).some(function (week) {
            if (week.start === weekStart) {
                result = week;
                return true;
            }
            return false;
        });
        return result;
    }

    function formatDays(value) {
        var number = Number(value) || 0;
        return number.toFixed(1).replace(".0", "") + " ngày";
    }

    function renderProject(container, project) {
        var card = document.createElement("div");
        card.className = "resource-drawer-project";

        var heading = document.createElement("div");
        heading.className = "resource-drawer-project-heading";
        appendText(
            heading,
            "div",
            "fw-semibold",
            (project.code || "Dự án") +
                (project.name ? " · " + project.name : ""));
        appendText(
            heading,
            "strong",
            Number(project.allocation) > 100
                ? "text-danger text-nowrap"
                : "text-primary text-nowrap",
            formatPercent(project.allocation));
        card.appendChild(heading);

        appendText(
            card,
            "div",
            "small text-muted mt-1",
            formatDays(project.allocatedDays) + " · " +
                (Number(project.taskCount) || 0) + " công việc");
        container.appendChild(card);
    }

    function renderTask(container, task) {
        var card = document.createElement("div");
        card.className = "resource-drawer-task";

        appendText(
            card,
            "div",
            "resource-drawer-task-project",
            (task.projectCode || "Dự án") +
                (task.projectName ? " · " + task.projectName : ""));
        appendText(
            card,
            "div",
            "fw-semibold mt-1",
            (task.code || "Công việc") +
                (task.name ? " - " + task.name : ""));

        var meta = document.createElement("div");
        meta.className = "resource-drawer-task-meta";
        appendText(
            meta,
            "span",
            "",
            formatDays(task.allocatedDays) + " · " +
                (task.activeDates || []).join(", "));
        appendText(
            meta,
            "strong",
            "text-primary text-nowrap",
            formatPercent(task.allocation));
        card.appendChild(meta);
        container.appendChild(card);
    }

    function openDrawer(employeeId, weekStart) {
        var drawer = document.getElementById("resource-detail-drawer");
        var backdrop = document.getElementById("resource-detail-backdrop");
        var title = document.getElementById("resource-detail-title");
        var subtitle = document.getElementById("resource-detail-subtitle");
        var load = document.getElementById("resource-detail-load");
        var capacity = document.getElementById("resource-detail-capacity");
        var projectContainer = document.getElementById("resource-detail-projects");
        var taskContainer = document.getElementById("resource-detail-tasks");
        var employee = findEmployee(employeeId);
        var week = employee ? findWeek(employee, weekStart) : null;

        if (!drawer || !backdrop || !employee || !week ||
            !capacity || !projectContainer || !taskContainer) {
            return;
        }

        title.textContent = employee.name;
        subtitle.textContent = week.label + " · " + week.displayRange +
            (employee.jobTitle ? " · " + employee.jobTitle : "") +
            (employee.department ? " · " + employee.department : "");
        load.textContent = formatPercent(week.allocation);
        load.className = Number(week.allocation) > 100
            ? "text-danger"
            : Number(week.allocation) >= 80
                ? "text-warning"
                : Number(week.allocation) > 0
                    ? "text-success"
                    : "text-secondary";
        capacity.textContent = formatDays(week.allocatedDays) + "/" +
            formatDays(week.capacityDays) + " công suất" +
            (Number(week.overAllocatedDays) > 0
                ? " · vượt " + formatDays(week.overAllocatedDays)
                : "") +
            (Number(week.overlapDayCount) > 0
                ? " · " + week.overlapDayCount + " ngày chồng lịch"
                : "");
        projectContainer.textContent = "";
        taskContainer.textContent = "";

        if (!week.projects || week.projects.length === 0) {
            appendText(
                projectContainer,
                "div",
                "resource-drawer-empty",
                "Nhân sự chưa được phân bổ vào dự án trong tuần này.");
        } else {
            week.projects.forEach(function (project) {
                renderProject(projectContainer, project);
            });
        }

        if (!week.tasks || week.tasks.length === 0) {
            appendText(
                taskContainer,
                "div",
                "resource-drawer-empty",
                "Không có công việc trong tuần này.");
        } else {
            week.tasks.forEach(function (task) {
                renderTask(taskContainer, task);
            });
        }

        backdrop.hidden = false;
        window.setTimeout(function () {
            backdrop.classList.add("is-open");
            drawer.classList.add("is-open");
            drawer.setAttribute("aria-hidden", "false");
        }, 0);
        document.body.classList.add("resource-drawer-open");
    }

    function closeDrawer() {
        var drawer = document.getElementById("resource-detail-drawer");
        var backdrop = document.getElementById("resource-detail-backdrop");
        if (!drawer || !backdrop) {
            return;
        }

        drawer.classList.remove("is-open");
        backdrop.classList.remove("is-open");
        drawer.setAttribute("aria-hidden", "true");
        document.body.classList.remove("resource-drawer-open");
        window.setTimeout(function () {
            if (!backdrop.classList.contains("is-open")) {
                backdrop.hidden = true;
            }
        }, 200);
    }

    function bindDrawer() {
        var dashboard = document.querySelector(".dashboard-resource");
        var closeButton = document.getElementById("resource-detail-close");
        var backdrop = document.getElementById("resource-detail-backdrop");

        if (!dashboard) {
            return;
        }

        dashboard.addEventListener("click", function (event) {
            var target = event.target;
            while (target && target !== dashboard &&
                !target.classList.contains("resource-load-button")) {
                target = target.parentElement;
            }

            if (target && target.classList.contains("resource-load-button")) {
                openDrawer(
                    target.getAttribute("data-resource-person"),
                    target.getAttribute("data-resource-week"));
            }
        });

        if (closeButton) {
            closeButton.addEventListener("click", closeDrawer);
        }
        if (backdrop) {
            backdrop.addEventListener("click", closeDrawer);
        }
        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape") {
                closeDrawer();
            }
        });
    }

    function initialize() {
        renderTrendChart();
        bindDrawer();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initialize);
    } else {
        initialize();
    }
}());
