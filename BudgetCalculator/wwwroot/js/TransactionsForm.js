function InitializaeTransactionForm(url) {
	$("#OperationTypeId").change(async function () {

		const selectedValue = $(this).val();

		const response = await fetch(url, {
			method: 'POST',
			body: selectedValue,
			headers: { 'Content-Type': 'application/json' }
		});

		const json = await response.json();
		console.log(json);

		const options = json.map(category => `<option value=${category.value}>${category.text}</option>`);
		$("#CategoryId").html(options);
	})
}