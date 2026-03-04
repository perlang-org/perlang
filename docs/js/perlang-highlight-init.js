// This JavaScript enables syntax highlighting of Perlang code blocks, using hljs

function highlightPerlangBlocks(root) {
    if (typeof hljs === "undefined") {
        return;
    }

    (root || document)
        .querySelectorAll(
            "pre code.language-perlang, pre code.lang-perlang, pre code.perlang, .highlight code.language-perlang"
        )
        .forEach(function (block) {
            if (block.dataset.perlangHljsDone === "1") {
                return;
            }

            if (typeof hljs.highlightElement === "function") {
                hljs.highlightElement(block);
            }
            else if (typeof hljs.highlightBlock === "function") {
                hljs.highlightBlock(block);
            }

            block.dataset.perlangHljsDone = "1";
        });
}

document.addEventListener("DOMContentLoaded", function () {
    highlightPerlangBlocks(document);
});

// Material for MkDocs instant navigation swaps page content without full reload.
if (typeof document$ !== "undefined" && typeof document$.subscribe === "function") {
    document$.subscribe(function () {
        highlightPerlangBlocks(document);
    });
}
