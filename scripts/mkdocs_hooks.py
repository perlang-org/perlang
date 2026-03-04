import re
from html import escape
from pathlib import Path


CODE_PERLANG_PATTERN = re.compile(
    r"^\[!code-perlang\[[^\]]+\]\((?P<path>[^)]+)\)\]\s*$",
    re.MULTILINE,
)

FENCED_PERLANG_PATTERN = re.compile(
    r"```perlang\s*\n(?P<code>.*?)\n```",
    re.DOTALL,
)


def _render_perlang_html_block(content: str) -> str:
    escaped_content = escape(content.rstrip())
    return f'\n<pre><code class="language-perlang">{escaped_content}</code></pre>\n'


def _replace_code_perlang_tag(match: re.Match[str], page) -> str:
    source_page_path = Path(page.file.abs_src_path)
    code_path = (source_page_path.parent / match.group("path")).resolve()

    if not code_path.is_file():
        raise FileNotFoundError(
            f"Could not resolve code-perlang include '{match.group('path')}' "
            f"from '{source_page_path}'."
        )

    content = code_path.read_text(encoding="utf-8")
    return _render_perlang_html_block(content)


def on_page_markdown(markdown, *, page, config, files):
    transformed = CODE_PERLANG_PATTERN.sub(
        lambda match: _replace_code_perlang_tag(match, page),
        markdown,
    )

    return FENCED_PERLANG_PATTERN.sub(
        lambda match: _render_perlang_html_block(match.group("code")),
        transformed,
    )
