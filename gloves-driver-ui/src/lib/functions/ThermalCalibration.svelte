<script lang="ts" context="module">
	export let title = 'Thermal Calibration';
	export let description =
		'Change thermal pad temperature to test functionality. Value from -500 to 500. Negative amount = cold, positive amount = hot.';
</script>

<script lang="ts">
	import { writable } from 'svelte/store';
	import Select from '$lib/components/form/Select.svelte';
	import { make_http_request } from '../scripts/http';
	import { Severity, ToastStore } from '../stores/toast';

	const state = writable({
		loading: false,
		form: {
			left_hand: true,
			amount: 0
		}
	});

	const change_temp = async () => {
		try {
			console.log($state.form.left_hand);
			$state.loading = true;
			const path = '/functions/thermal_feedback/'.concat($state.form.left_hand ? 'left' : 'right');
			await make_http_request({
				path,
				method: 'POST',
				body: {
					value: $state.form.amount,
				}
			});

			ToastStore.add_toast(Severity.SUCCESS, 'Successfully updated thermals');
		} catch (e) {
			console.trace(e);

			ToastStore.add_toast(Severity.ERROR, e);
		} finally {
			$state.loading = false;
		}
	};
</script>

<Select
	options={[
		{ title: 'Left Hand', value: true },
		{ title: 'Right Hand', value: false }
	]}
	bind:selected_value={$state.form.left_hand}
	label="For Hand"
/>
<div class="mt-3 flex flex-col">
	<label for="thermal-temp">Value: {$state.form.amount}</label>
	<input
		type="range"
		min="-500"
		max="500"
		bind:value={$state.form.amount}
		class="range range-xs"
		id="thermal-temp"
		on:mouseup={() => {
			change_temp();
		}}
	/>
</div>
<div class="mt-3">
	<p class="mb-2">or:</p>
	<button
		class="btn btn-sm btn-success"
		on:click={() => {
			$state.form.amount = 0;
			change_temp();
		}}
		>Turn Off Thermals
	</button>
</div>