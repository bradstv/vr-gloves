<script lang="ts" context="module">
	export let title = 'Haptic Calibration';
	export let description =
		'Turn on/off haptics for a cartain amount of ms to test functionality.';
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

	const enable_haptics = async () => {
		try {
			console.log($state.form.left_hand);
			$state.loading = true;
			const path = '/functions/haptic_feedback/'.concat($state.form.left_hand ? 'left' : 'right');
			await make_http_request({
				path,
				method: 'POST',
				body: {
					thumb: $state.form.amount,
					index: $state.form.amount,
					middle: $state.form.amount,
					ring: $state.form.amount,
					pinky: $state.form.amount
				}
			});

			ToastStore.add_toast(Severity.SUCCESS, 'Successfully updated haptics');
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
	<label for="haptic-time">Duration (ms): {$state.form.amount}</label>
	<input
		type="range"
		min="0"
		max="10000"
		bind:value={$state.form.amount}
		class="range range-xs"
		id="haptic-time"
		on:mouseup={() => {
			enable_haptics();
		}}
	/>
</div>
<div class="mt-3">
	<p class="mb-2">or:</p>
	<button
		class="btn btn-sm btn-success"
		on:click={() => {
			$state.form.amount = 1;
			enable_haptics();
		}}
		>Turn Off Haptics
	</button>
</div>
