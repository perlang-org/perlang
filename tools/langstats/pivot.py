#!/usr/bin/env python3
"""
Pivots the raw langstats CSV (date,language,category,lines,files) into a wide
table of cumulative sums (one column per language+category) suitable for a
stacked-area plot in gnuplot, plus a matching gnuplot 'plot' command with
per-column colors and titles.

Columns are grouped by language (largest language total first) and, within a
language, ordered own -> vendored -> auto-generated. Colors are a shade
family per language: own is the most saturated/darkest shade, vendored and
auto-generated get progressively lighter tints of the same hue.
"""

import argparse
import colorsys
import csv
from collections import defaultdict

DISPLAY_NAME = {
    "csharp": "C#",
    "cpp": "C++",
    "c": "C",
    "perlang": "Perlang",
}

# Hue in degrees (0-360) per language.
LANGUAGE_HUE = {
    "csharp": 280,  # purple
    "cpp": 150,  # green
    "c": 205,  # blue
    "perlang": 35,  # orange
}

CATEGORY_ORDER = ["own", "vendored", "auto-generated"]
CATEGORY_LIGHTNESS = {
    "own": 0.45,
    "vendored": 0.62,
    "auto-generated": 0.80,
}


def column_title(language, category):
    name = DISPLAY_NAME.get(language, language)
    return name if category == "own" else f"{name} ({category})"


def hex_color(hue_degrees, lightness, saturation=0.55):
    r, g, b = colorsys.hls_to_rgb(hue_degrees / 360, lightness, saturation)
    return "#{:02x}{:02x}{:02x}".format(round(r * 255), round(g * 255), round(b * 255))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("input", help="Raw langstats CSV")
    parser.add_argument("--output", default="langstats_stacked.tsv")
    parser.add_argument("--gp-output", default="plot_commands.gp.inc")
    args = parser.parse_args()

    totals = defaultdict(lambda: defaultdict(int))  # date -> (language, category) -> lines
    keys_seen = set()

    with open(args.input, newline="") as f:
        for row in csv.DictReader(f):
            key = (row["language"], row["category"])
            totals[row["date"]][key] += int(row["lines"])
            keys_seen.add(key)

    language_totals = defaultdict(int)
    for date_totals in totals.values():
        for (language, category), lines in date_totals.items():
            language_totals[language] += lines

    languages_ordered = sorted(language_totals, key=lambda lang: language_totals[lang], reverse=True)

    columns = []  # [(language, category), ...] in final column order
    for language in languages_ordered:
        categories_here = [c for c in CATEGORY_ORDER if (language, c) in keys_seen]
        for category in categories_here:
            columns.append((language, category))

    dates = sorted(totals)

    with open(args.output, "w") as f:
        headers = [column_title(lang, cat) for lang, cat in columns]
        f.write("date\t" + "\t".join(headers) + "\n")
        for date in dates:
            cumulative = 0
            values = []
            for key in columns:
                cumulative += totals[date].get(key, 0)
                values.append(str(cumulative))
            f.write(date + "\t" + "\t".join(values) + "\n")

    with open(args.gp_output, "w") as f:
        plot_parts = []
        prev_col = None
        for i, (language, category) in enumerate(columns):
            col = i + 2  # column 1 is date
            title = column_title(language, category)
            color = hex_color(LANGUAGE_HUE.get(language, 0), CATEGORY_LIGHTNESS.get(category, 0.6))
            if prev_col is None:
                using = f"1:{col}"
                style = "filledcurves x1"
            else:
                using = f"1:{col}:{prev_col}"
                style = "filledcurves"
            plot_parts.append(f"    input using {using} with {style} title '{title}' lc rgb '{color}'")
            prev_col = col
        f.write("plot \\\n" + ", \\\n".join(plot_parts) + "\n")

    print(f"Wrote {args.output} and {args.gp_output} ({len(dates)} rows, {len(columns)} stacked series)")


if __name__ == "__main__":
    main()
