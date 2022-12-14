import{_ as e,a,b as t,c as r}from"./chunks/search-panel.9e61f3cc.js";import{_ as s,o,c as i,a as n}from"./app.3c3613bb.js";const h="/GH_SizeAnalyzer/assets/view-simple.dd44d054.png",z=JSON.parse('{"title":"Introduction","description":"","frontmatter":{},"headers":[{"level":2,"title":"What problem does it try to solve?","slug":"what-problem-does-it-try-to-solve","link":"#what-problem-does-it-try-to-solve","children":[]},{"level":2,"title":"Previous solutions","slug":"previous-solutions","link":"#previous-solutions","children":[{"level":3,"title":"De-activate auto-save","slug":"de-activate-auto-save","link":"#de-activate-auto-save","children":[]},{"level":3,"title":"Reference data instead of internalizing","slug":"reference-data-instead-of-internalizing","link":"#reference-data-instead-of-internalizing","children":[]}]},{"level":2,"title":"How does GH_SizeAnalyzer help?","slug":"how-does-gh-sizeanalyzer-help","link":"#how-does-gh-sizeanalyzer-help","children":[{"level":3,"title":"Features","slug":"features","link":"#features","children":[]}]}],"relativePath":"intro.md"}'),d={name:"intro.md"},c=n('<h1 id="introduction" tabindex="-1">Introduction <a class="header-anchor" href="#introduction" aria-hidden="true">#</a></h1><p><code>GH_SizeAnalyzer</code> aims to provide a quick way to find the Grasshopper nodes that have <strong>too much data</strong> internalized.</p><p><img src="'+h+'" alt="Parameter warning"></p><h2 id="what-problem-does-it-try-to-solve" tabindex="-1">What problem does it try to solve? <a class="header-anchor" href="#what-problem-does-it-try-to-solve" aria-hidden="true">#</a></h2><p>There are 2 Grasshopper features that tend to clash with each other when used incorrectly.</p><ul><li>The ability to internalize data inside any parameter</li><li>The ability to auto-save the document whenever anything changes.</li></ul><p>Under normal circumstances, these 2 features work as expected. The problem comes when a user starts to internalize <strong>too much data</strong> inside different Grasshopper parameters within a document.</p><p>As a Grasshopper document grows, the internalized data can start <strong>slowing down</strong> the auto-save process, as it needs to save all that internal data too! This appears to the user as a delay between any change of component wires, which freezes Grasshopper for a perceptible amount of time.</p><p>The problem becomes even more frustrating because by the time you realize this, you may have dozens of nodes that need manual checking to identify which ones have to be slimmed down.</p><div class="tip custom-block"><p class="custom-block-title">💡</p><p>This is not a flaw on McNeel&#39;s side, it was just never designed to store a lot of data within the Grasshopper document.</p></div><h2 id="previous-solutions" tabindex="-1">Previous solutions <a class="header-anchor" href="#previous-solutions" aria-hidden="true">#</a></h2><h3 id="de-activate-auto-save" tabindex="-1">De-activate auto-save <a class="header-anchor" href="#de-activate-auto-save" aria-hidden="true">#</a></h3><p>A quick search on McNeel&#39;s forum returns several user&#39;s bumping into this throughout the years, with the overall recommendation being to <strong>deactivate auto-save</strong>.</p><p><a href="https://discourse.mcneel.com/search?q=grasshopper%20autosave%20slow" target="_blank" rel="noreferrer">https://discourse.mcneel.com/search?q=grasshopper autosave slow</a></p><p>This recommendation is a recipe for disaster, as surely some work will be lost at some point of that document&#39;s history due to the fact that auto-save was disabled in pro of usability.</p><h3 id="reference-data-instead-of-internalizing" tabindex="-1">Reference data instead of internalizing <a class="header-anchor" href="#reference-data-instead-of-internalizing" aria-hidden="true">#</a></h3><p>You can prevent the auto-save from slowing down if instead the data is stored in an outside source, which can be a Rhino document, or a plain old text file. Rhino is much better at storing data in an <code>rhp</code> file than Grasshopper is at doing the same in a <code>gh</code> file.</p><p>Once the data is no longer stored in Grasshopper, you can reference back either by using <code>referenced objects</code>, a <code>grasshpper pipeline</code> or even <code>data input/output</code> nodes.</p><div class="tip custom-block"><p class="custom-block-title">💡 TIP:</p><p>This would be our recommended approach, and the reason this widget exists 🙂</p></div><h2 id="how-does-gh-sizeanalyzer-help" tabindex="-1">How does <code>GH_SizeAnalyzer</code> help? <a class="header-anchor" href="#how-does-gh-sizeanalyzer-help" aria-hidden="true">#</a></h2><p><code>GH_SizeAnalyzer</code> is a Grasshopper plugin that provides a new <strong>widget</strong> under the <code>Widgets</code> section.</p><p><img src="'+e+'" alt="Widget&#39;s section"></p><h3 id="features" tabindex="-1">Features <a class="header-anchor" href="#features" aria-hidden="true">#</a></h3><p>The widget will draw 2 new warnings in the Grasshopper canvas:</p><h4 id="document-size-warning" tabindex="-1">Document size warning <a class="header-anchor" href="#document-size-warning" aria-hidden="true">#</a></h4><p>A red capsule that is drawn on the bottom-left corner of the Grasshopper canvas whenever the total size of internal data in a given document exceeds a specific user-defined threshold.</p><p><img src="'+a+'" alt="Document warning"></p><h4 id="parameter-size-warning" tabindex="-1">Parameter size warning <a class="header-anchor" href="#parameter-size-warning" aria-hidden="true">#</a></h4><p>A small red badge that is drawn on the top-right corner of any given parameter (or component input) whose internal data size exceeds a specific user-defined threshold.</p><p><img src="'+t+'" alt="Parameter warning"></p><h4 id="parameter-search" tabindex="-1">Parameter search <a class="header-anchor" href="#parameter-search" aria-hidden="true">#</a></h4><p>Similar to Grasshopper&#39;s <code>F3</code> Search panel. This search panel allows the user to focus on the parameters that have the largest size quickly, and to further highlight any parameters that exceed the size threshold so they can be easily found.</p><p><img src="'+r+'" alt="Search panel"></p>',33),l=[c];function p(u,m,g,f,v,w){return o(),i("div",null,l)}const y=s(d,[["render",p]]);export{z as __pageData,y as default};