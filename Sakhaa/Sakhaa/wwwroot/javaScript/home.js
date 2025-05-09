        document.addEventListener("DOMContentLoaded", () => {
  const statsSection = document.querySelector("#statisticsCards");
  const counters = statsSection.querySelectorAll("p:first-child");

  const options = {
    root: null, 
    threshold: 0.5, 
  };

  const animateCounters = (entries, observer) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        counters.forEach((counter) => {
          const updateCount = () => {
            const target = +counter.dataset.target;
            const current = +counter.innerText;
            const increment = Math.ceil(target / 50); 

            if (current < target) {
              counter.innerText = current + increment;
              setTimeout(updateCount, 30); 
            } else {
              counter.innerText = target; 
            }
          };

          if (!counter.dataset.started) {
            counter.dataset.started = true;
            updateCount();
          }
        });

        observer.disconnect();
      }
    });
  };

  const observer = new IntersectionObserver(animateCounters, options);

  observer.observe(statsSection);

  counters[0].dataset.target = 25; 
  counters[1].dataset.target = 50; 
  counters[2].dataset.target = 1000; 
});


/////////////////////


