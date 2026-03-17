(function () {
  const theme = localStorage.getItem("theme") || "light";
  document.documentElement.setAttribute("data-bs-theme", theme);
})();

function toggleTheme() {
  const html = document.documentElement;
  const current = html.getAttribute("data-bs-theme") || "light";
  const next = current === "light" ? "dark" : "light";
  html.setAttribute("data-bs-theme", next);
  localStorage.setItem("theme", next);
  document.getElementById("themeIcon").textContent =
    next === "dark" ? "☀️" : "🌙";
}

document.addEventListener("DOMContentLoaded", function () {
  const theme = localStorage.getItem("theme") || "light";
  const icon = document.getElementById("themeIcon");
  if (icon) icon.textContent = theme === "dark" ? "☀️" : "🌙";
});
