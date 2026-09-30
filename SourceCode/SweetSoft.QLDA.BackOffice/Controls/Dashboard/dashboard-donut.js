(function (root) {
    "use strict";

    function share(percent) {
        return percent > 0 && percent < 1 ? "<1%" : Math.round(percent) + "%";
    }

    function refresh(element, chart, values, colors, totalText, totalLabel) {
        var svg = element && element.querySelector("svg");
        var slices = svg && svg.querySelectorAll(".apexcharts-pie-series path");
        if (!svg || !slices || !slices.length) { return; }

        var center = element.querySelector(".dashboard-donut-center");
        if (!center) {
            center = document.createElement("span");
            center.className = "dashboard-donut-center";
            var number = document.createElement("strong");
            var caption = document.createElement("small");
            center.appendChild(number);
            center.appendChild(caption);
            element.appendChild(center);
        }
        center.querySelector("strong").textContent = totalText;
        center.querySelector("small").textContent = totalLabel;

        var pieBox = null;
        Array.prototype.forEach.call(slices, function (slice) {
            var box = slice.getBoundingClientRect();
            if (!box.width || !box.height) { return; }
            if (!pieBox) {
                pieBox = { left: box.left, right: box.right, top: box.top, bottom: box.bottom };
            } else {
                pieBox.left = Math.min(pieBox.left, box.left);
                pieBox.right = Math.max(pieBox.right, box.right);
                pieBox.top = Math.min(pieBox.top, box.top);
                pieBox.bottom = Math.max(pieBox.bottom, box.bottom);
            }
        });
        if (!pieBox) { return; }
        var elementBox = element.getBoundingClientRect();
        center.style.left = ((pieBox.left + pieBox.right) / 2 - elementBox.left) + "px";
        center.style.top = ((pieBox.top + pieBox.bottom) / 2 - elementBox.top) + "px";

        Array.prototype.forEach.call(svg.querySelectorAll("[data-dashboard-donut-connector]"), function (line) {
            line.parentNode.removeChild(line);
        });
        var labels = Array.prototype.slice.call(svg.querySelectorAll("text.apexcharts-pie-label"));
        var globals = chart && chart.w && chart.w.globals;
        if (!globals || !labels.length) { return; }

        var gridWidth = Number(globals.gridWidth) || element.clientWidth;
        var gridHeight = Number(globals.gridHeight) || element.clientHeight;
        var radius = Number(globals.radialSize) || Math.min(gridWidth, gridHeight) / 2;
        var donutSize = parseInt(chart.w.config.plotOptions.pie.donut.size, 10) || 64;
        var labelRadius = radius * (1 + donutSize / 100) / 2;
        var total = values.reduce(function (sum, value) { return sum + value; }, 0);
        var sweep = 0;
        var labelIndex = 0;
        var outside = [];
        values.forEach(function (value, index) {
            if (value <= 0) { return; }
            var degrees = total ? 360 * value / total : 0;
            var label = labels[labelIndex++];
            if (!label) { sweep += degrees; return; }
            var angle = (sweep + degrees / 2 - 90) * Math.PI / 180;
            var dx = Math.cos(angle);
            var dy = Math.sin(angle);
            var x = gridWidth / 2;
            var y = Math.min(gridWidth, gridHeight) / 2;
            label.classList.remove("dashboard-donut-external-label");
            label.style.fill = "";
            if (degrees < 14) {
                label.setAttribute("x", x + dx * labelRadius);
                label.setAttribute("y", y + dy * labelRadius);
                label.setAttribute("text-anchor", "middle");
                outside.push({
                    label: label, color: colors[index], dx: dx, dy: dy,
                    x: x + dx * (radius + 18), y: y + dy * (radius + 18),
                    cx: x, cy: y, radius: radius, side: dx >= 0 ? "right" : "left"
                });
            }
            sweep += degrees;
        });

        ["left", "right"].forEach(function (side) {
            var items = outside.filter(function (item) { return item.side === side; })
                .sort(function (a, b) { return a.y - b.y; });
            items.forEach(function (item, index) {
                item.y = Math.max(12, item.y);
                if (index) { item.y = Math.max(item.y, items[index - 1].y + 16); }
            });
            if (items.length && items[items.length - 1].y > gridHeight - 8) {
                var shift = items[items.length - 1].y - gridHeight + 8;
                items.forEach(function (item) { item.y -= shift; });
            }
        });
        outside.forEach(function (item) {
            var label = item.label;
            var width = Math.max(14, (label.textContent || "").length * 6.5);
            var anchor = item.side === "right" ? "start" : "end";
            var x = item.x;
            if (anchor === "start") { x = Math.min(x, Math.max(gridWidth, element.clientWidth) - width - 4); }
            else { x = Math.max(x, width + 4); }
            var line = svg.ownerDocument.createElementNS("http://www.w3.org/2000/svg", "path");
            line.setAttribute("data-dashboard-donut-connector", "true");
            line.setAttribute("class", "dashboard-donut-connector");
            line.setAttribute("d", "M " + (item.cx + item.dx * (item.radius + 2)) + " "
                + (item.cy + item.dy * (item.radius + 2)) + " L "
                + (x + (anchor === "start" ? -3 : 3)) + " " + item.y);
            line.setAttribute("fill", "none");
            line.setAttribute("stroke", item.color || "#64748b");
            label.parentNode.insertBefore(line, label);
            label.setAttribute("x", x);
            label.setAttribute("y", item.y);
            label.setAttribute("text-anchor", anchor);
            label.style.fill = "#45546a";
            label.classList.add("dashboard-donut-external-label");
        });
    }

    function schedule(element, chart, values, colors, totalText, totalLabel) {
        window.setTimeout(function () {
            refresh(element, chart, values, colors, totalText, totalLabel);
        }, 0);
    }

    root.DashboardDonut = { share: share, schedule: schedule };
}(window));
