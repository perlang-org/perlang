PYTHON ?= python3
GNUPLOT ?= gnuplot

OUTPUT_DIR := output
CSV := $(OUTPUT_DIR)/langstats.csv
TSV := $(OUTPUT_DIR)/langstats_stacked.tsv
PLOT_COMMANDS := $(OUTPUT_DIR)/plot_commands.gp.inc
PNG := $(OUTPUT_DIR)/langstats.png

.PHONY: all csv clean

all: $(PNG)

$(OUTPUT_DIR):
	mkdir -p $(OUTPUT_DIR)

# Reflects the full (and ever-growing) git history, so it's always
# regenerated rather than treated as up to date once present.
.PHONY: $(CSV)
$(CSV): | $(OUTPUT_DIR)
	$(PYTHON) langstats.py --output $(CSV)

$(TSV) $(PLOT_COMMANDS) &: $(CSV) pivot.py
	$(PYTHON) pivot.py $(CSV) --output $(TSV) --gp-output $(PLOT_COMMANDS)

$(PNG): $(TSV) $(PLOT_COMMANDS) plot.gp
	$(GNUPLOT) -c plot.gp $(TSV) $(PNG) $(PLOT_COMMANDS)

csv: $(CSV)

clean:
	rm -f $(CSV) $(TSV) $(PLOT_COMMANDS) $(PNG)
