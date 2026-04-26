# Wiki Source

This folder contains GitHub Wiki-ready Markdown pages for the project.

The actual GitHub Wiki lives in a separate git repository:

```text
https://github.com/bpopovgit/bullet-heaven-game.wiki.git
```

Use these files as the maintained source copy, then publish them to the wiki repo.

## Publishing From Git Bash

```bash
cp "/d/Bullet Heaven Game/Docs/WikiSource/"*.md "/d/bullet-heaven-game.wiki/"
cd "/d/bullet-heaven-game.wiki"
git status
git add .
git commit -m "Update wiki for current Unity 6 game systems"
git push
```
