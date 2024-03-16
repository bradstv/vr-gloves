<script lang="ts" context="module">
	export let title = 'Finger Calibration Data';
	export let description =
		'Save or Clear current finger calibration data on device.';
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
		}
	});

	const save_calib_data = async (inter: boolean, travel: boolean, clear: boolean) => {
		try {
			console.log($state.form.left_hand);
			$state.loading = true;
			const path = '/functions/finger_calibration/'.concat($state.form.left_hand ? 'left' : 'right');
			await make_http_request({
				path,
				method: 'POST',
				body: {
					save_inter: inter,
					save_travel: travel,
					clear_data: clear,
				}
			});

			ToastStore.add_toast(Severity.SUCCESS, 'Successfully updated finger calibration data');
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
<div class="mt-3">
	<button
		class="btn btn-sm btn-info"
		on:click={() => {
			save_calib_data(true, true, false);
		}}
		>Save Finger Calibration
	</button>
	<button
		class="btn btn-sm btn-success"
		on:click={() => {
			save_calib_data(false, false, true);
		}}
		>Clear Finger Calibration
	</button>
</div>