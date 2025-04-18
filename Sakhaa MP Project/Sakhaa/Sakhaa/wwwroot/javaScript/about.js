const slider = document.querySelector(".slider");
        const slides = document.querySelectorAll(".slide");

        let index = 0;
        const totalSlides = slides.length;
        const slideWidth = 100 / 4; 

        function updateSlider() {
            slider.style.transform = `translateX(-${index * slideWidth}%)`;
        }

        function autoSlide() {
            if (index < totalSlides - 4) { 
                index++;
            } else {
                index = 0; 
            }
            updateSlider();
        }

        setInterval(autoSlide, 3000); 