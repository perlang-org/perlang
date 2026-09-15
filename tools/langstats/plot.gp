# Usage: gnuplot -c plot.gp <input.tsv> <output.png> <plot_commands.gp.inc>

input = ARG1
output = ARG2
plot_commands = ARG3

set terminal pngcairo size 1200,700 enhanced font "sans,11"
set output output

set title "Perlang lines of code over time"
set xdata time
set timefmt "%Y-%m-%d"
set format x "%Y-%m"
set xlabel "Date"
set ylabel "Lines of code"
set key left top
set grid ytics
set style fill solid 0.85 border -1

# Milestones: date, label, vertical stagger (0 = near top, 1 = a bit lower,
# to reduce label collisions between nearby dates).
array MILESTONE_DATE[6] = ["2023-09-27", "2024-02-27", "2024-04-26", "2024-09-18", "2025-01-14", "2025-02-24"]
array MILESTONE_LABEL[6] = ["fmt vendored", "libtommath vendored", "interpreter removed", "CLI rewrite begins", "CppSharp bindings", "tsl vendored"]
array MILESTONE_STAGGER[6] = [0, 1, 0, 1, 0, 1]

do for [i=1:6] {
    set arrow from MILESTONE_DATE[i],graph 0 to MILESTONE_DATE[i],graph 1 nohead lc rgb '#555555' dt 2 lw 1
    set label MILESTONE_LABEL[i] at MILESTONE_DATE[i],graph (0.97 - MILESTONE_STAGGER[i]*0.06) rotate by 90 font "sans,8" tc rgb '#333333' right offset 0,0
}

load plot_commands
