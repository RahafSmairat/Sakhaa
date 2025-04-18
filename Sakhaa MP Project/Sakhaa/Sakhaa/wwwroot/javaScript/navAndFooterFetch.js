fetch('navbar.html')
.then(response => response.text())
.then(data => document.getElementById('nav').innerHTML = data);

fetch('footer.html')
.then(response => response.text())
.then(data => document.getElementById('footer').innerHTML = data);


////////////

document.addEventListener("DOMContentLoaded", function () {
    const checkNavbar = setInterval(() => {
        const navbar = document.querySelector(".navbar"); 
        if (navbar) {
            navbar.classList.add("visible");
            clearInterval(checkNavbar); 
        }
    }, 100); 
});

/////////////////////////

document.addEventListener("DOMContentLoaded", function () {
    const sections = document.querySelectorAll(".section");
  
    function revealSections() {
        sections.forEach(section => {
            const sectionTop = section.getBoundingClientRect().top;
            const windowHeight = window.innerHeight;
            if (sectionTop < windowHeight - 100) {
                section.classList.add("visible");
            }
        });
    }
    window.addEventListener("scroll", revealSections);
    revealSections();
  });