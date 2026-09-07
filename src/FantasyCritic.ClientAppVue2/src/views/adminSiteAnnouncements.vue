<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Site announcements</h1>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
    </div>
    <hr />

    <div v-show="errorInfo" class="alert alert-danger">Request for '{{ jobAttempted }}' returned: {{ errorInfo }}</div>
    <div v-show="errorResponse" class="alert alert-danger">{{ errorResponse }}</div>
    <div v-show="lastJobFailed" class="alert alert-danger">'{{ jobAttempted }}' failed.</div>
    <div v-show="isBusy" class="alert alert-info">Request is processing...</div>
    <div v-show="jobAttempted && !lastJobFailed && !isBusy" class="alert alert-success">'{{ jobAttempted }}' successfully run.</div>

    <h2>{{ isEditing ? 'Edit announcement' : 'New announcement' }}</h2>
    <div class="form-group">
      <label for="announcementTitle">Title</label>
      <input id="announcementTitle" v-model="form.title" type="text" class="form-control" maxlength="255" />
    </div>
    <div class="form-group">
      <label for="announcementBody">Body</label>
      <textarea id="announcementBody" v-model="form.body" class="form-control" rows="5"></textarea>
      <small class="text-muted">Plain text. Line breaks are preserved when the announcement is displayed.</small>
    </div>
    <div class="form-group">
      <label for="announcementPostedAt">Posted at (in your local time zone)</label>
      <flat-pickr id="announcementPostedAt" v-model="form.postedAt" :config="datePickerConfig" class="form-control"></flat-pickr>
      <small class="text-muted">Announcements are shown newest first. Leave blank on a new announcement to use the current time.</small>
    </div>
    <div class="form-row">
      <div class="form-group col-md-8">
        <label for="announcementLinkAddress">Link address (optional)</label>
        <input id="announcementLinkAddress" v-model="form.linkAddress" type="text" class="form-control" />
      </div>
      <div class="form-group col-md-4">
        <label for="announcementLinkLabel">Link label (optional)</label>
        <input id="announcementLinkLabel" v-model="form.linkLabel" type="text" class="form-control" maxlength="255" />
      </div>
    </div>
    <div>
      <b-button v-if="isEditing" variant="primary" :disabled="!formIsValid || isBusy" @click="saveEdit">Save changes</b-button>
      <b-button v-else variant="success" :disabled="!formIsValid || isBusy" @click="createAnnouncement">Post announcement</b-button>
      <b-button v-if="isEditing" variant="secondary" :disabled="isBusy" @click="clearForm">Cancel</b-button>
    </div>

    <h2 class="mt-5">Posted announcements</h2>
    <div>
      <b-button variant="info" class="mb-2" :disabled="isBusy" @click="refreshAnnouncements">Refresh</b-button>
    </div>
    <p v-if="announcements && !announcements.length" class="text-muted">No announcements have been posted yet.</p>
    <b-table v-else-if="announcements" :items="announcements" :fields="announcementFields" striped bordered responsive>
      <template #cell(postedAt)="row">{{ row.item.postedAt | dateTime }}</template>
      <template #cell(link)="row">
        <b-link v-if="row.item.linkAddress" :href="row.item.linkAddress" target="_blank" rel="noopener">{{ row.item.linkLabel || row.item.linkAddress }}</b-link>
      </template>
      <template #cell(actions)="row">
        <b-button variant="primary" size="sm" :disabled="isBusy" @click="startEdit(row.item)">Edit</b-button>
        <b-button variant="danger" size="sm" :disabled="isBusy" @click="confirmDelete(row.item)">Delete</b-button>
      </template>
    </b-table>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  data() {
    return {
      isBusy: false,
      errorInfo: null,
      errorResponse: null,
      lastJobFailed: false,
      jobAttempted: '',
      announcements: null,
      editingAnnouncementID: null,
      form: {
        title: '',
        body: '',
        postedAt: null,
        linkAddress: '',
        linkLabel: ''
      },
      datePickerConfig: {
        enableTime: true
      },
      announcementFields: [
        { key: 'title', label: 'Title', thClass: 'bg-primary' },
        { key: 'postedAt', label: 'Posted', thClass: 'bg-primary' },
        { key: 'link', label: 'Link', thClass: 'bg-primary' },
        { key: 'actions', label: '', thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    isEditing() {
      return !!this.editingAnnouncementID;
    },
    formIsValid() {
      return !!this.form.title.trim() && !!this.form.body.trim();
    }
  },
  async mounted() {
    // Load quietly - the initial page load isn't a "job" the admin asked for, so it gets no result banner.
    try {
      await this.fetchAnnouncements();
    } catch (error) {
      this.jobAttempted = 'Get site announcements';
      this.errorInfo = error;
      this.errorResponse = error.response;
      this.lastJobFailed = true;
    }
  },
  methods: {
    clearForm() {
      this.editingAnnouncementID = null;
      this.form = {
        title: '',
        body: '',
        postedAt: null,
        linkAddress: '',
        linkLabel: ''
      };
    },
    startEdit(announcement) {
      this.editingAnnouncementID = announcement.id;
      this.form = {
        title: announcement.title,
        body: announcement.body,
        postedAt: new Date(announcement.postedAt),
        linkAddress: announcement.linkAddress || '',
        linkLabel: announcement.linkLabel || ''
      };
      window.scrollTo({ top: 0, behavior: 'smooth' });
    },
    async fetchAnnouncements() {
      const response = await axios.get('/api/admin/GetSiteAnnouncements');
      this.announcements = response.data;
    },
    async refreshAnnouncements() {
      this.lastJobFailed = false;
      this.jobAttempted = 'Get site announcements';
      this.isBusy = true;

      try {
        await this.fetchAnnouncements();
      } catch (error) {
        this.errorInfo = error;
        this.errorResponse = error.response;
        this.lastJobFailed = true;
      } finally {
        this.isBusy = false;
      }
    },
    async createAnnouncement() {
      if (!this.formIsValid) {
        return;
      }

      this.lastJobFailed = false;
      this.jobAttempted = 'Create site announcement';
      this.isBusy = true;

      try {
        await axios.post('/api/admin/CreateSiteAnnouncement', {
          title: this.form.title,
          body: this.form.body,
          postedAt: this.form.postedAt ? new Date(this.form.postedAt).toISOString() : null,
          linkAddress: this.form.linkAddress,
          linkLabel: this.form.linkLabel
        });
        this.clearForm();
        await this.fetchAnnouncements();
      } catch (error) {
        this.errorInfo = error;
        this.errorResponse = error.response;
        this.lastJobFailed = true;
      } finally {
        this.isBusy = false;
      }
    },
    async saveEdit() {
      if (!this.formIsValid || !this.editingAnnouncementID) {
        return;
      }

      this.lastJobFailed = false;
      this.jobAttempted = 'Edit site announcement';
      this.isBusy = true;

      try {
        await axios.post('/api/admin/EditSiteAnnouncement', {
          announcementID: this.editingAnnouncementID,
          title: this.form.title,
          body: this.form.body,
          postedAt: new Date(this.form.postedAt).toISOString(),
          linkAddress: this.form.linkAddress,
          linkLabel: this.form.linkLabel
        });
        this.clearForm();
        await this.fetchAnnouncements();
      } catch (error) {
        this.errorInfo = error;
        this.errorResponse = error.response;
        this.lastJobFailed = true;
      } finally {
        this.isBusy = false;
      }
    },
    async confirmDelete(announcement) {
      const confirmed = await this.$bvModal.msgBoxConfirm(`Are you sure you want to delete '${announcement.title}'? It will no longer show on the site or in the RSS feed.`, {
        title: 'Delete Announcement',
        okTitle: 'Delete',
        okVariant: 'danger',
        cancelTitle: 'Cancel'
      });
      if (!confirmed) {
        return;
      }

      this.lastJobFailed = false;
      this.jobAttempted = 'Delete site announcement';
      this.isBusy = true;

      try {
        await axios.post('/api/admin/DeleteSiteAnnouncement', {
          announcementID: announcement.id
        });
        if (this.editingAnnouncementID === announcement.id) {
          this.clearForm();
        }
        await this.fetchAnnouncements();
      } catch (error) {
        this.errorInfo = error;
        this.errorResponse = error.response;
        this.lastJobFailed = true;
      } finally {
        this.isBusy = false;
      }
    }
  }
};
</script>
