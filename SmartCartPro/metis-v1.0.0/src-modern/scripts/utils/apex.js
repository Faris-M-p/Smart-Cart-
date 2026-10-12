// ==========================================================================
// ApexCharts — modular build
// ==========================================================================
//
// ApexCharts 7 ships per-chart-type entry points instead of one barrel. The
// barrel (`import ApexCharts from 'apexcharts'`) pulls in every chart type and
// every optional feature — boxplot, candlestick, violin, sunburst, heatmap,
// drilldown, storyboard, annotations, the canvas renderer… none of which this
// template renders.
//
// Importing a chart-type module registers it on the core via `core.use()`, so
// each import below is a side-effect import. Keep this list in sync with the
// chart types actually used in scripts/components/*.js — adding a new chart
// type to a component means adding its module here, or the chart silently
// fails to render.
//
// Currently rendered:  area, line, bar, donut, polarArea, radar, radialBar, treemap
//
import ApexCharts from 'apexcharts/core';

// Chart types. Several modules register more than one type:
import 'apexcharts/area'; // → line, area, scatter, bubble, rangeArea
import 'apexcharts/bar'; // → bar, column, barStacked, rangeBar
import 'apexcharts/donut'; // → pie, donut, polarArea
import 'apexcharts/radar'; // → radar
import 'apexcharts/radialBar'; // → radialBar
import 'apexcharts/treemap'; // → treemap

// Optional features. `legend` is built into core; these are not.
import 'apexcharts/features/toolbar'; // toolbar + zoomPanSelection
import 'apexcharts/features/exports'; // toolbar `download` tool (SVG/PNG/CSV)

export default ApexCharts;
