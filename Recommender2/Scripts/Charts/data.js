﻿/*
 Highcharts JS v5.0.0 (2016-09-29)
 Data module

 (c) 2012-2016 Torstein Honsi

 License: www.highcharts.com/license
*/
(function (q) { "object" === typeof module && module.exports ? module.exports = q : q(Highcharts) })(function (q) {
    (function (g) {
        var q = g.win.document, m = g.each, A = g.pick, x = g.inArray, y = g.isNumber, B = g.splat, n, w = function (b, a) { this.init(b, a) }; g.extend(w.prototype, {
            init: function (b, a) {
                this.options = b; this.chartOptions = a; this.columns = b.columns || this.rowsToColumns(b.rows) || []; this.firstRowAsNames = A(b.firstRowAsNames, !0); this.decimalRegex = b.decimalPoint && new RegExp("^(-?[0-9]+)" + b.decimalPoint + "([0-9]+)$"); this.rawColumns = [];
                this.columns.length ? this.dataFound() : (this.parseCSV(), this.parseTable(), this.parseGoogleSpreadsheet())
            }, getColumnDistribution: function () {
                var b = this.chartOptions, a = this.options, e = [], f = function (b) { return (g.seriesTypes[b || "line"].prototype.pointArrayMap || [0]).length }, d = b && b.chart && b.chart.type, c = [], h = [], v = 0, k; m(b && b.series || [], function (b) { c.push(f(b.type || d)) }); m(a && a.seriesMapping || [], function (b) { e.push(b.x || 0) }); 0 === e.length && e.push(0); m(a && a.seriesMapping || [], function (a) {
                    var e = new n, t, r = c[v] || f(d),
                    p = g.seriesTypes[((b && b.series || [])[v] || {}).type || d || "line"].prototype.pointArrayMap || ["y"]; e.addColumnReader(a.x, "x"); for (t in a) a.hasOwnProperty(t) && "x" !== t && e.addColumnReader(a[t], t); for (k = 0; k < r; k++) e.hasReader(p[k]) || e.addColumnReader(void 0, p[k]); h.push(e); v++
                }); a = g.seriesTypes[d || "line"].prototype.pointArrayMap; void 0 === a && (a = ["y"]); this.valueCount = { global: f(d), xColumns: e, individual: c, seriesBuilders: h, globalPointArrayMap: a }
            }, dataFound: function () {
                this.options.switchRowsAndColumns && (this.columns =
                this.rowsToColumns(this.columns)); this.getColumnDistribution(); this.parseTypes(); !1 !== this.parsed() && this.complete()
            }, parseCSV: function () {
                var b = this, a = this.options, e = a.csv, f = this.columns, d = a.startRow || 0, c = a.endRow || Number.MAX_VALUE, h = a.startColumn || 0, v = a.endColumn || Number.MAX_VALUE, k, g, z = 0; e && (g = e.replace(/\r\n/g, "\n").replace(/\r/g, "\n").split(a.lineDelimiter || "\n"), k = a.itemDelimiter || (-1 !== e.indexOf("\t") ? "\t" : ","), m(g, function (a, e) {
                    var g = b.trim(a), u = 0 === g.indexOf("#"); e >= d && e <= c && !u && "" !== g && (g =
                    a.split(k), m(g, function (b, a) { a >= h && a <= v && (f[a - h] || (f[a - h] = []), f[a - h][z] = b) }), z += 1)
                }), this.dataFound())
            }, parseTable: function () { var b = this.options, a = b.table, e = this.columns, f = b.startRow || 0, d = b.endRow || Number.MAX_VALUE, c = b.startColumn || 0, h = b.endColumn || Number.MAX_VALUE; a && ("string" === typeof a && (a = q.getElementById(a)), m(a.getElementsByTagName("tr"), function (b, a) { a >= f && a <= d && m(b.children, function (b, d) { ("TD" === b.tagName || "TH" === b.tagName) && d >= c && d <= h && (e[d - c] || (e[d - c] = []), e[d - c][a - f] = b.innerHTML) }) }), this.dataFound()) },
            parseGoogleSpreadsheet: function () {
                var b = this, a = this.options, e = a.googleSpreadsheetKey, f = this.columns, d = a.startRow || 0, c = a.endRow || Number.MAX_VALUE, h = a.startColumn || 0, g = a.endColumn || Number.MAX_VALUE, k, u; e && jQuery.ajax({
                    dataType: "json", url: "https://spreadsheets.google.com/feeds/cells/" + e + "/" + (a.googleSpreadsheetWorksheet || "od6") + "/public/values?alt=json-in-script&callback=?", error: a.error, success: function (a) {
                        a = a.feed.entry; var e, r = a.length, p = 0, n = 0, l; for (l = 0; l < r; l++) e = a[l], p = Math.max(p, e.gs$cell.col),
                        n = Math.max(n, e.gs$cell.row); for (l = 0; l < p; l++) l >= h && l <= g && (f[l - h] = [], f[l - h].length = Math.min(n, c - d)); for (l = 0; l < r; l++) e = a[l], k = e.gs$cell.row - 1, u = e.gs$cell.col - 1, u >= h && u <= g && k >= d && k <= c && (f[u - h][k - d] = e.content.$t); m(f, function (a) { for (l = 0; l < a.length; l++) void 0 === a[l] && (a[l] = null) }); b.dataFound()
                    }
                })
            }, trim: function (b, a) { "string" === typeof b && (b = b.replace(/^\s+|\s+$/g, ""), a && /^[0-9\s]+$/.test(b) && (b = b.replace(/\s/g, "")), this.decimalRegex && (b = b.replace(this.decimalRegex, "$1.$2"))); return b }, parseTypes: function () {
                for (var b =
                this.columns, a = b.length; a--;) this.parseColumn(b[a], a)
            }, parseColumn: function (b, a) {
                var e = this.rawColumns, f = this.columns, d = b.length, c, h, g, k, n = this.firstRowAsNames, m = -1 !== x(a, this.valueCount.xColumns), t = [], r = this.chartOptions, p, q = (this.options.columnTypes || [])[a], r = m && (r && r.xAxis && "category" === B(r.xAxis)[0].type || "string" === q); for (e[a] || (e[a] = []) ; d--;) c = t[d] || b[d], g = this.trim(c), k = this.trim(c, !0), h = parseFloat(k), void 0 === e[a][d] && (e[a][d] = g), r || 0 === d && n ? b[d] = g : +k === h ? (b[d] = h, 31536E6 < h && "float" !== q ? b.isDatetime =
                !0 : b.isNumeric = !0, void 0 !== b[d + 1] && (p = h > b[d + 1])) : (h = this.parseDate(c), m && y(h) && "float" !== q ? (t[d] = c, b[d] = h, b.isDatetime = !0, void 0 !== b[d + 1] && (c = h > b[d + 1], c !== p && void 0 !== p && (this.alternativeFormat ? (this.dateFormat = this.alternativeFormat, d = b.length, this.alternativeFormat = this.dateFormats[this.dateFormat].alternative) : b.unsorted = !0), p = c)) : (b[d] = "" === g ? null : g, 0 !== d && (b.isDatetime || b.isNumeric) && (b.mixed = !0))); m && b.mixed && (f[a] = e[a]); if (m && p && this.options.sort) for (a = 0; a < f.length; a++) f[a].reverse(), n && f[a].unshift(f[a].pop())
            },
            dateFormats: {
                "YYYY-mm-dd": { regex: /^([0-9]{4})[\-\/\.]([0-9]{2})[\-\/\.]([0-9]{2})$/, parser: function (b) { return Date.UTC(+b[1], b[2] - 1, +b[3]) } }, "dd/mm/YYYY": { regex: /^([0-9]{1,2})[\-\/\.]([0-9]{1,2})[\-\/\.]([0-9]{4})$/, parser: function (b) { return Date.UTC(+b[3], b[2] - 1, +b[1]) }, alternative: "mm/dd/YYYY" }, "mm/dd/YYYY": { regex: /^([0-9]{1,2})[\-\/\.]([0-9]{1,2})[\-\/\.]([0-9]{4})$/, parser: function (b) { return Date.UTC(+b[3], b[1] - 1, +b[2]) } }, "dd/mm/YY": {
                    regex: /^([0-9]{1,2})[\-\/\.]([0-9]{1,2})[\-\/\.]([0-9]{2})$/,
                    parser: function (b) { return Date.UTC(+b[3] + 2E3, b[2] - 1, +b[1]) }, alternative: "mm/dd/YY"
                }, "mm/dd/YY": { regex: /^([0-9]{1,2})[\-\/\.]([0-9]{1,2})[\-\/\.]([0-9]{2})$/, parser: function (b) { return Date.UTC(+b[3] + 2E3, b[1] - 1, +b[2]) } }
            }, parseDate: function (b) {
                var a = this.options.parseDate, e, f, d = this.options.dateFormat || this.dateFormat, c; if (a) e = a(b); else if ("string" === typeof b) {
                    if (d) a = this.dateFormats[d], (c = b.match(a.regex)) && (e = a.parser(c)); else for (f in this.dateFormats) if (a = this.dateFormats[f], c = b.match(a.regex)) {
                        this.dateFormat =
                        f; this.alternativeFormat = a.alternative; e = a.parser(c); break
                    } c || (c = Date.parse(b), "object" === typeof c && null !== c && c.getTime ? e = c.getTime() - 6E4 * c.getTimezoneOffset() : y(c) && (e = c - 6E4 * (new Date(c)).getTimezoneOffset()))
                } return e
            }, rowsToColumns: function (b) { var a, e, f, d, c; if (b) for (c = [], e = b.length, a = 0; a < e; a++) for (d = b[a].length, f = 0; f < d; f++) c[f] || (c[f] = []), c[f][a] = b[a][f]; return c }, parsed: function () { if (this.options.parsed) return this.options.parsed.call(this, this.columns) }, getFreeIndexes: function (b, a) {
                var e, f, d =
                [], c = [], h; for (f = 0; f < b; f += 1) d.push(!0); for (e = 0; e < a.length; e += 1) for (h = a[e].getReferencedColumnIndexes(), f = 0; f < h.length; f += 1) d[h[f]] = !1; for (f = 0; f < d.length; f += 1) d[f] && c.push(f); return c
            }, complete: function () {
                var b = this.columns, a, e = this.options, f, d, c, h, g = [], k; if (e.complete || e.afterComplete) {
                    for (c = 0; c < b.length; c++) this.firstRowAsNames && (b[c].name = b[c].shift()); f = []; d = this.getFreeIndexes(b.length, this.valueCount.seriesBuilders); for (c = 0; c < this.valueCount.seriesBuilders.length; c++) k = this.valueCount.seriesBuilders[c],
                    k.populateColumns(d) && g.push(k); for (; 0 < d.length;) { k = new n; k.addColumnReader(0, "x"); c = x(0, d); -1 !== c && d.splice(c, 1); for (c = 0; c < this.valueCount.global; c++) k.addColumnReader(void 0, this.valueCount.globalPointArrayMap[c]); k.populateColumns(d) && g.push(k) } 0 < g.length && 0 < g[0].readers.length && (k = b[g[0].readers[0].columnIndex], void 0 !== k && (k.isDatetime ? a = "datetime" : k.isNumeric || (a = "category"))); if ("category" === a) for (c = 0; c < g.length; c++) for (k = g[c], d = 0; d < k.readers.length; d++) "x" === k.readers[d].configName && (k.readers[d].configName =
                    "name"); for (c = 0; c < g.length; c++) { k = g[c]; d = []; for (h = 0; h < b[0].length; h++) d[h] = k.read(b, h); f[c] = { data: d }; k.name && (f[c].name = k.name); "category" === a && (f[c].turboThreshold = 0) } b = { series: f }; a && (b.xAxis = { type: a }, "category" === a && (b.xAxis.nameToX = !1)); e.complete && e.complete(b); e.afterComplete && e.afterComplete(b)
                }
            }
        }); g.Data = w; g.data = function (b, a) { return new w(b, a) }; g.wrap(g.Chart.prototype, "init", function (b, a, e) {
            var f = this; a && a.data ? g.data(g.extend(a.data, {
                afterComplete: function (d) {
                    var c, h; if (a.hasOwnProperty("series")) if ("object" ===
                    typeof a.series) for (c = Math.max(a.series.length, d.series.length) ; c--;) h = a.series[c] || {}, a.series[c] = g.merge(h, d.series[c]); else delete a.series; a = g.merge(d, a); b.call(f, a, e)
                }
            }), a) : b.call(f, a, e)
        }); n = function () { this.readers = []; this.pointIsArray = !0 }; n.prototype.populateColumns = function (b) { var a = !0; m(this.readers, function (a) { void 0 === a.columnIndex && (a.columnIndex = b.shift()) }); m(this.readers, function (b) { void 0 === b.columnIndex && (a = !1) }); return a }; n.prototype.read = function (b, a) {
            var e = this.pointIsArray, f = e ?
            [] : {}, d; m(this.readers, function (c) { var d = b[c.columnIndex][a]; e ? f.push(d) : f[c.configName] = d }); void 0 === this.name && 2 <= this.readers.length && (d = this.getReferencedColumnIndexes(), 2 <= d.length && (d.shift(), d.sort(), this.name = b[d.shift()].name)); return f
        }; n.prototype.addColumnReader = function (b, a) { this.readers.push({ columnIndex: b, configName: a }); "x" !== a && "y" !== a && void 0 !== a && (this.pointIsArray = !1) }; n.prototype.getReferencedColumnIndexes = function () {
            var b, a = [], e; for (b = 0; b < this.readers.length; b += 1) e = this.readers[b],
            void 0 !== e.columnIndex && a.push(e.columnIndex); return a
        }; n.prototype.hasReader = function (b) { var a, e; for (a = 0; a < this.readers.length; a += 1) if (e = this.readers[a], e.configName === b) return !0 }
    })(q)
});