document.addEventListener('DOMContentLoaded', function() {
    // Multi-step form variables
    let currentTab = 0;
    const tabs = document.getElementsByClassName("tab");
    const nextBtn = document.getElementById("nextBtn");
    const prevBtn = document.getElementById("prevBtn");
    const steps = document.getElementsByClassName("step");
    
    // Initialize form
    showTab(currentTab);
    
    // Amount selector logic
    const amountOptions = document.querySelectorAll('.amount-option');
    const customAmountContainer = document.querySelector('.custom-amount-container');
    const customAmountInput = document.getElementById('customAmount');
    const selectedAmountInput = document.getElementById('selectedAmount');
    
    // Set default selected amount
    document.querySelector('.amount-option[data-amount="50"]').classList.add('selected');
    
    // Add click event listeners to all amount options
    amountOptions.forEach(option => {
        option.addEventListener('click', function() {
            console.log('Amount option clicked:', this.getAttribute('data-amount'));
            
            // Remove selected class from all options
            amountOptions.forEach(opt => opt.classList.remove('selected'));
            
            // Add selected class to this option
            this.classList.add('selected');
            
            const amount = this.getAttribute('data-amount');
            
            if (amount === 'custom') {
                // Show custom amount input
                customAmountContainer.classList.remove('hidden');
                customAmountInput.focus();
                
                // If custom amount already has a value, use it
                if (customAmountInput.value) {
                    selectedAmountInput.value = customAmountInput.value;
                }
            } else {
                // Hide custom amount input
                customAmountContainer.classList.add('hidden');
                
                // Update selectedAmountInput with the selected amount
                selectedAmountInput.value = amount;
                
                // Update preview
                updateCardPreview();
            }
        });
    });
    
    // Update selected amount when custom amount changes
    customAmountInput.addEventListener('input', function() {
        console.log('Custom amount input:', this.value);
        // Update hidden input with custom amount
        selectedAmountInput.value = this.value;
        console.log('Hidden amount input updated to:', selectedAmountInput.value);
        
        // Update preview
        updateCardPreview();
    });
    
    // Character counter for message
    const personalMessage = document.getElementById("personalMessage");
    const charCount = document.getElementById("charCount");
    
    personalMessage.addEventListener("input", function() {
        const count = this.value.length;
        charCount.textContent = count;
        updateCardPreview();
    });
    
    // Gift card customization
    const occasionOptions = document.querySelectorAll(".occasion-option");
    const colorOptions = document.querySelectorAll(".color-option");
    const decorationOptions = document.querySelectorAll(".decoration-option");
    
    const selectedOccasion = document.getElementById("selectedOccasion");
    const selectedColor = document.getElementById("selectedColor");
    const selectedDecoration = document.getElementById("selectedDecoration");
    
    // Set default selected options
    document.querySelector('.occasion-option[data-occasion="general"]').classList.add("selected");
    document.querySelector('.color-option[data-color="#000000"]').classList.add("selected");
    document.querySelector('.decoration-option[data-decoration="none"]').classList.add("selected");
    
    // Occasion selection
    occasionOptions.forEach(option => {
        option.addEventListener("click", function() {
            // Remove selected class from all options
            occasionOptions.forEach(opt => opt.classList.remove("selected"));
            // Add selected class to clicked option
            this.classList.add("selected");
            // Update hidden input
            const occasion = this.getAttribute("data-occasion");
            selectedOccasion.value = occasion;
            // Update preview
            updateCardPreview();
        });
    });
    
    // Color selection
    colorOptions.forEach(option => {
        option.addEventListener("click", function() {
            // Remove selected class from all options
            colorOptions.forEach(opt => opt.classList.remove("selected"));
            // Add selected class to clicked option
            this.classList.add("selected");
            // Update hidden input
            const color = this.getAttribute("data-color");
            selectedColor.value = color;
            // Update preview
            updateCardPreview();
        });
    });
    
    // Decoration selection
    decorationOptions.forEach(option => {
        option.addEventListener("click", function() {
            // Remove selected class from all options
            decorationOptions.forEach(opt => opt.classList.remove("selected"));
            // Add selected class to clicked option
            this.classList.add("selected");
            // Update hidden input
            const decoration = this.getAttribute("data-decoration");
            selectedDecoration.value = decoration;
            // Update preview
            updateCardPreview();
        });
    });
    
    // Name inputs for preview
    const giverName = document.getElementById("giverName");
    const receiverName = document.getElementById("receiverName");
    
    giverName.addEventListener("input", updateCardPreview);
    receiverName.addEventListener("input", updateCardPreview);
    
    // Show/hide amount
    const showAmountYes = document.getElementById("showAmountYes");
    const showAmountNo = document.getElementById("showAmountNo");
    
    showAmountYes.addEventListener("change", updateCardPreview);
    showAmountNo.addEventListener("change", updateCardPreview);
    
    // Navigation buttons
    nextBtn.addEventListener("click", function() {
        nextPrev(1);
    });
    
    prevBtn.addEventListener("click", function() {
        nextPrev(-1);
    });
    
    // Form submission - capture the form's submit event
    const giftDonationForm = document.getElementById("giftDonationForm");
    giftDonationForm.addEventListener("submit", function(e) {
        // Prevent form submission if validation fails
        if (currentTab < (tabs.length - 1)) {
            e.preventDefault();
            nextPrev(1);
            return;
        }
        
        // Get the selected option
        const selectedOption = document.querySelector('.amount-option.selected');
        
        // If custom amount is selected, update the hidden input field
        if (selectedOption && selectedOption.getAttribute('data-amount') === 'custom') {
            if (customAmountInput.value && parseFloat(customAmountInput.value) >= 10) {
                console.log('Setting hidden amount field to custom value:', customAmountInput.value);
                selectedAmountInput.value = customAmountInput.value;
            }
        } else if (selectedOption) {
            // Otherwise use the standard amount
            console.log('Setting hidden amount field to standard value:', selectedOption.getAttribute('data-amount'));
            selectedAmountInput.value = selectedOption.getAttribute('data-amount');
        }
        
        console.log('Form submitting with amount:', selectedAmountInput.value);
        
        // Image capture is now handled in the view with html2canvas
        // The original form submission is being prevented there, so we don't need to do anything else here
    });
    
    // Functions
    function showTab(n) {
        // Display the specified tab
        for (let i = 0; i < tabs.length; i++) {
            tabs[i].style.display = "none";
        }
        tabs[n].style.display = "flex";
        
        // Update button visibility
        if (n === 0) {
            prevBtn.style.display = "none";
        } else {
            prevBtn.style.display = "inline";
        }
        
        // Change button text on last step
        if (n === (tabs.length - 1)) {
            nextBtn.innerHTML = "انتقل إلى الدفع";
        } else {
            nextBtn.innerHTML = "التالي";
        }
        
        // Update step indicators
        updateStepIndicator(n);
    }
    
    function nextPrev(n) {
        // Exit if any required field is not filled
        if (n === 1 && !validateForm()) return false;
        
        // Hide current tab
        tabs[currentTab].style.display = "none";
        
        // Increase or decrease current tab
        currentTab = currentTab + n;
        
        // If reached end of form, submit
        if (currentTab >= tabs.length) {
            document.getElementById("giftDonationForm").submit();
            return false;
        }
        
        // Otherwise display the correct tab
        showTab(currentTab);
    }
    
    function validateForm() {
        let valid = true;
        const tab = tabs[currentTab];
        const inputs = tab.querySelectorAll("input[required], select[required], textarea[required]");
        
        // Special check for amount when on the donation tab (tab index 1)
        if (currentTab === 1) {
            // Get the selected amount option
            const selectedOption = document.querySelector('.amount-option.selected');
            
            if (!selectedOption) {
                // No amount option selected
                valid = false;
                alert('الرجاء اختيار مبلغ التبرع');
            } else if (selectedOption.getAttribute('data-amount') === 'custom') {
                // Custom amount selected - validate the input
                if (!customAmountInput.value || customAmountInput.value < 10) {
                    customAmountInput.classList.add("invalid");
                    valid = false;
                    alert('الرجاء إدخال مبلغ لا يقل عن 10 د.أ');
                } else {
                    customAmountInput.classList.remove("invalid");
                    // Make sure the selectedAmount hidden input is updated
                    selectedAmountInput.value = customAmountInput.value;
                }
            } else {
                // Standard amount selected - make sure the hidden input is updated
                selectedAmountInput.value = selectedOption.getAttribute('data-amount');
            }
        }
        
        // Check all required fields in current tab
        inputs.forEach(input => {
            if (input.value === "") {
                // Add "invalid" class
                input.classList.add("invalid");
                valid = false;
            } else {
                input.classList.remove("invalid");
            }
        });
        
        // If valid, mark step as completed
        if (valid) {
            steps[currentTab].classList.add("finish");
        }
        
        return valid;
    }
    
    function updateStepIndicator(n) {
        // Remove "active" class from all steps
        for (let i = 0; i < steps.length; i++) {
            steps[i].classList.remove("active");
        }
        
        // Add "active" class to current step
        steps[n].classList.add("active");
    }
    
    function updateCardPreview() {
        // Update names
        const fromName = giverName.value || "...";
        const toName = receiverName.value || "...";
        document.getElementById("previewFrom").textContent = fromName;
        document.getElementById("previewTo").textContent = toName;
        
        // Update message
        const message = personalMessage.value || "رسالتك ستظهر هنا...";
        document.getElementById("previewMessage").textContent = message;
        
        // Update amount display
        const amountValue = getAmount();
        const amountElement = document.getElementById("previewAmount");
        
        if (showAmountYes.checked) {
            amountElement.style.display = "block";
            amountElement.innerHTML = "مبلغ التبرع: <span>" + amountValue + " د.أ</span>";
        } else {
            amountElement.style.display = "none";
        }
        
        // Update occasion image
        const occasion = selectedOccasion.value;
        document.getElementById("occasionImage").src = "/images/gift/" + occasion + ".png";
        
        // Update text color
        const color = selectedColor.value;
        document.getElementById("giftCardHeader").style.color = color;
        
        // Update decoration
        const decoration = selectedDecoration.value;
        const giftCardPreview = document.getElementById("giftCardPreview");
        
        // Remove any existing background decoration
        const existingBgDecoration = document.querySelector(".card-background-decoration");
        if (existingBgDecoration) {
            existingBgDecoration.remove();
        }
        
        if (decoration === "none") {
            // No decoration
        } else {
            // Add decoration as a background to the entire card
            const bgDecoration = document.createElement("div");
            bgDecoration.className = "card-background-decoration";
            bgDecoration.style.backgroundImage = "url('/images/gift/decoration-" + decoration + ".png')";
            giftCardPreview.insertBefore(bgDecoration, giftCardPreview.firstChild);
        }
    }
    
    function getAmount() {
        // Get amount from hidden input or from active selection
        const selectedAmount = selectedAmountInput.value || "0";
        
        // For custom amounts, check if the input has a value
        const selectedOption = document.querySelector('.amount-option.selected');
        if (selectedOption && selectedOption.getAttribute('data-amount') === 'custom') {
            return customAmountInput.value || selectedAmount;
        }
        
        return selectedAmount;
    }
    
    // Initialize preview
    updateCardPreview();
});