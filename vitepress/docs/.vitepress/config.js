export default {
  //base: "/GH_SizeAnalyzer/",
  title: "GH Size Analyzer",
  description:
    "A grasshopper component to analyze size of internal data in stored in the document.",
  themeConfig: {
    footer: {
      message:
        'Released under the <a href="https://github.com/vuejs/vitepress/blob/main/LICENSE">MIT License</a>.',
      copyright:
        'Copyright © 2022-present <a href="https://github.com/paramdigma">Paramdigma</a>'
    },
    sidebar: [
      {
        text: "Introduction",
        items: [{ text: "What is GH_SizeAnalyzer?", link: "/intro.md" }]
      },
      {
        text: "Getting started",
        items: [
          { text: "Install", link: "/install.md" },
          { text: "Usage", link: "/usage.md" },
          { text: "Settings", link: "/settings.md" }
        ]
      },
      { text: "Examples", items: [{ text: "Basic", link: "/examples.md" }] }
    ],
    nav: [
      { text: "About us", link: "/team" },
      {
        text: "Changelog",
        link: "https://github.com/paramdigma/gh_sizeanalyzer/releases"
      }
    ]
  }
}
