// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.getElementById("navbar-toggler").addEventListener("click", (e) => {
  console.log(e);
  const button = document.getElementById("navbar-toggler");
  const currentState = button.getAttribute("data-state");

  console.log(currentState);
  if (!currentState || currentState === "closed") {
    button.setAttribute("data-state", "opened");
    button.setAttribute("aria-expanded", "true");
  } else {
    button.setAttribute("data-state", "closed");
    button.setAttribute("aria-expanded", "false");
  }
  const navbar = document.getElementById("navbar-collapse");
  navbar.classList.toggle("hide-navbar");
  navbar.classList.toggle("show");
});
