<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">Conference</div>
  </div>
</template>
<script>
export default {
  props: {
    conferenceid: { type: String, required: true },
    year: { type: Number, required: true }
  }
};
</script>
