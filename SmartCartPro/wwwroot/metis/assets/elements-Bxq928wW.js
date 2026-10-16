const __vite__mapDeps=(i,m=__vite__mapDeps,d=(m.f||(m.f=["./prism-CYH2nXNs.js","./rolldown-runtime-hePW80VL.js","./prism-XjOTrbqP.css"])))=>i.map(i=>d[i]);
import{n as e}from"./vendor-ui-Be8JR3ju.js";import{t}from"./main-1Eks1J0_.js";import{t as n}from"./search-component-7FwxNuMO.js";var r=[{id:`buttons`,title:`Buttons`,category:`components`,icon:`bi bi-square`,description:`Bootstrap button styles, sizes, and states`,examples:12,url:`/elements-buttons.html`,preview:`
            <div class="d-flex gap-2 flex-wrap">
                <button class="btn btn-primary btn-sm">Primary</button>
                <button class="btn btn-outline-secondary btn-sm">Secondary</button>
                <button class="btn btn-success btn-sm">Success</button>
            </div>
        `},{id:`alerts`,title:`Alerts`,category:`components`,icon:`bi bi-exclamation-triangle`,description:`Contextual feedback messages for user actions`,examples:8,url:`/elements-alerts.html`,preview:`
            <div class="alert alert-primary alert-sm py-2 px-3 mb-2" role="alert">
                <i class="bi bi-info-circle me-2"></i>Primary alert
            </div>
            <div class="alert alert-success alert-sm py-2 px-3 mb-0" role="alert">
                <i class="bi bi-check-circle me-2"></i>Success alert
            </div>
        `},{id:`badges`,title:`Badges`,category:`components`,icon:`bi bi-award`,description:`Small count and labeling components`,examples:6,url:`/elements-badges.html`,preview:`
            <div class="d-flex gap-2 flex-wrap">
                <span class="badge bg-primary">Primary</span>
                <span class="badge bg-secondary">Secondary</span>
                <span class="badge bg-success">Success</span>
                <span class="badge bg-danger">Danger</span>
            </div>
        `},{id:`cards`,title:`Cards`,category:`components`,icon:`bi bi-card-text`,description:`Flexible content containers with headers and footers`,examples:10,url:`/elements-cards.html`,preview:`
            <div class="card" style="width: 200px;">
                <div class="card-body p-3">
                    <h6 class="card-title mb-2">Card Title</h6>
                    <p class="card-text mb-2 small">Sample card content with text.</p>
                    <button class="btn btn-primary btn-sm">Action</button>
                </div>
            </div>
        `},{id:`modals`,title:`Modals`,category:`components`,icon:`bi bi-window`,description:`Streamlined modal dialogs with flexible content`,examples:9,url:`/elements-modals.html`,preview:`
            <button class="btn btn-primary btn-sm" data-bs-toggle="modal" data-bs-target="#exampleModal">
                Launch Modal
            </button>
            <div class="modal fade" id="exampleModal" tabindex="-1" style="display: none;">
                <div class="modal-dialog modal-sm">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h6 class="modal-title">Modal Title</h6>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <p class="mb-0 small">Modal content goes here.</p>
                        </div>
                    </div>
                </div>
            </div>
        `},{id:`forms`,title:`Form Controls`,category:`forms`,icon:`bi bi-ui-checks-grid`,description:`Form inputs, selects, checkboxes, and validation`,examples:15,url:`/elements-forms.html`,preview:`
            <div class="mb-2">
                <input type="text" class="form-control form-control-sm" placeholder="Text input">
            </div>
            <div class="mb-2">
                <select class="form-select form-select-sm">
                    <option>Choose...</option>
                    <option>Option 1</option>
                </select>
            </div>
            <div class="form-check">
                <input class="form-check-input" type="checkbox" checked>
                <label class="form-check-label small">Check me</label>
            </div>
        `},{id:`tables`,title:`Tables`,category:`content`,icon:`bi bi-table`,description:`Responsive tables with various styling options`,examples:8,url:`/elements-tables.html`,preview:`
            <table class="table table-sm">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>John Doe</td>
                        <td><span class="badge bg-success">Active</span></td>
                    </tr>
                    <tr>
                        <td>Jane Smith</td>
                        <td><span class="badge bg-warning">Pending</span></td>
                    </tr>
                </tbody>
            </table>
        `}];document.addEventListener(`alpine:init`,()=>{e.data(`elementsComponent`,()=>({components:r,filteredComponents:r,viewMode:`grid`,searchQuery:``,categoryFilter:``,init(){this.filteredComponents=this.components,console.log(`🧩 Elements component initialized`)},filterComponents(){let e=this.components;if(this.searchQuery){let t=this.searchQuery.toLowerCase();e=e.filter(e=>e.title.toLowerCase().includes(t)||e.description.toLowerCase().includes(t)||e.category.toLowerCase().includes(t))}this.categoryFilter&&(e=e.filter(e=>e.category===this.categoryFilter)),this.filteredComponents=e},toggleView(){this.viewMode=this.viewMode===`grid`?`list`:`grid`,localStorage.setItem(`elements-view-mode`,this.viewMode)},navigateToComponent(e){window.location.href=e.url},showAllComponents(){this.searchQuery=``,this.categoryFilter=``,this.filteredComponents=this.components},clearFilters(){this.searchQuery=``,this.categoryFilter=``,this.filterComponents()},getComponentCount(e){return this.components.filter(t=>t.category===e).length}})),e.data(`searchComponent`,n({getResults(e){let t=e.toLowerCase(),n=r.filter(e=>e.title.toLowerCase().includes(t)||e.description.toLowerCase().includes(t)).map(e=>({title:e.title,url:e.url,type:`element`})),i=[{title:`Dashboard`,url:`/`,type:`page`},{title:`Analytics`,url:`/analytics`,type:`page`},{title:`Users`,url:`/users`,type:`page`},{title:`Elements`,url:`/elements`,type:`page`}].filter(e=>e.title.toLowerCase().includes(t));return[...n,...i].slice(0,8)}}))});var i=2e3;function a(e,t=`Copied!`){if(e.dataset.copyBusy===`true`)return;e.dataset.copyBusy=`true`;let n=Array.from(e.childNodes),r=document.createElement(`i`);r.className=`bi bi-check me-2`,e.replaceChildren(r,document.createTextNode(t)),e.classList.add(`btn-success`),setTimeout(()=>{e.replaceChildren(...n),e.classList.remove(`btn-success`),delete e.dataset.copyBusy},i)}async function o(e){if(navigator.clipboard?.writeText)try{return await navigator.clipboard.writeText(e),!0}catch{}let t=document.createElement(`textarea`);t.value=e,t.setAttribute(`readonly`,``),t.style.cssText=`position:absolute;left:-9999px;top:0;`,document.body.appendChild(t),t.select();let n;try{n=document.execCommand(`copy`)}catch{n=!1}return t.remove(),n}function s(e){return(e.closest(`.element-example, .card, section`)??e.parentElement)?.querySelector(`.element-code-block pre code`)??null}function c(e,t){let n=document.getElementById(`liveAlertPlaceholder`);if(!n)return;let r=document.createElement(`div`);r.className=`alert alert-${t} alert-dismissible`,r.setAttribute(`role`,`alert`);let i=document.createElement(`div`);i.textContent=e,r.appendChild(i);let a=document.createElement(`button`);a.type=`button`,a.className=`btn-close`,a.setAttribute(`data-bs-dismiss`,`alert`),a.setAttribute(`aria-label`,`Close`),r.appendChild(a),n.appendChild(r)}var l={primary:`This is a primary alert message!`,success:`Success! Your action was completed.`,warning:`Warning! Please check your input.`,danger:`Error! Something went wrong.`};function u(e){let t=e.target.closest(`[data-copy-code]`);if(t){let e=s(t);e&&o(e.textContent).then(e=>e&&a(t));return}let n=e.target.closest(`[data-copy-all]`);if(n){let e=document.querySelectorAll(`.element-code-block pre code`);o(Array.from(e).map(e=>e.textContent).join(`

`)).then(e=>e&&a(n,`All copied!`));return}let r=e.target.closest(`[data-show-alert]`);if(r){let e=r.dataset.showAlert;c(l[e]??`Alert message`,e);return}let i=e.target.closest(`[data-faq-toggle]`);if(i){let e=i.nextElementSibling?.classList.toggle(`show`)??!1;i.setAttribute(`aria-expanded`,String(e));return}e.target.closest(`[data-history-back]`)&&window.history.back()}function d(){document.querySelectorAll(`.needs-validation`).forEach(e=>{e.addEventListener(`submit`,t=>{e.checkValidity()||(t.preventDefault(),t.stopPropagation()),e.classList.add(`was-validated`)},!1)})}function f(e){document.readyState===`loading`?document.addEventListener(`DOMContentLoaded`,e,{once:!0}):e()}f(()=>{document.addEventListener(`click`,u),d(),document.querySelector(`pre code[class*="language-"]`)&&t(async()=>{let{highlightAll:e}=await import(`./prism-CYH2nXNs.js`);return{highlightAll:e}},__vite__mapDeps([0,1,2]),import.meta.url).then(({highlightAll:e})=>e()).catch(e=>console.error(`Prism failed to load:`,e))});export{r as elementsData};