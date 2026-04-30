document.getElementById("loginForm").addEventListener("submit", function (e) {

    let empId = document.querySelector("input[name='EmpId']").value.trim();
    let password = document.querySelector("input[name='Password']").value.trim();

    if (empId === "" || password === "") {
        e.preventDefault();
        alert("Please enter both Employee ID and Password");
    }
});
