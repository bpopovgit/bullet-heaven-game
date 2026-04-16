# Wiki Source

This folder contains GitHub Wiki-ready Markdown pages for the project.

The actual GitHub Wiki lives in a separate git repository:

```text
https://github.com/bpopovgit/bullet-heaven-game.wiki.git
```

Use these files as the maintained source copy, then publish them to the wiki repo.

## Publishing

After enabling the Wiki in GitHub:

```powershell
git clone https://github.com/bpopovgit/bullet-heaven-game.wiki.git
Copy-Item -Path "Docs\WikiSource\*.md" -Destination "bullet-heaven-game.wiki" -Force
cd bullet-heaven-game.wiki
git add .
git commit -m "Add initial project wiki"
git push
```

