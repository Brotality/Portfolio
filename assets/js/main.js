hljs.initHighlightingOnLoad();

var currentBody = {};
var currentTab = {};

document.getElementById("tab-navigation-main").getElementsByTagName("a")[0].click();
document.getElementById("tab-navigation-snippets").getElementsByTagName("a")[0].click();

function openTab(evt, type, bodyName) {
    if(type in currentBody)
        currentBody[type].style.display = "none";
    if(type in currentTab)
        currentTab[type].className = "";
    currentBody[type] = document.getElementById(bodyName);
    currentBody[type].style.display = "block";
    currentTab[type] = evt.currentTarget;
    currentTab[type].className = "selected";
}
