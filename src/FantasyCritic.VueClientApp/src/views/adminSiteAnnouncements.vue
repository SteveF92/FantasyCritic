<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Site announcements</h1>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
    </div>
    <hr />

    <b-alert v-if="errorInfo" variant="danger" :model-value="true">'{{ jobAttempted }}' failed: {{ errorInfo }}</b-alert>
    <b-alert v-else-if="isBusy" variant="info" :model-value="true">Request is processing...</b-alert>
    <b-alert v-else-if="jobAttempted" variant="success" :model-value="true">'{{ jobAttempted }}' successfully run.</b-alert>

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
      <input id="announcementPostedAt" v-model="form.postedAt" type="datetime-local" class="form-control" />
      <small class="text-muted">Announcements are shown newest first. Leave blank on a new announcement to use the current time. An edited announcement needs one.</small>
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
    <b-table v-else-if="announcements" :items="announcements" :fields="announcementFields" primary-key="id" striped bordered responsive>
      <template #cell(postedAt)="row">{{ formatDateTime(row.item.postedAt) }}</template>
      <template #cell(link)="row">
        <b-link v-if="row.item.linkAddress" :href="row.item.linkAddress" target="_blank" rel="noopener">{{ row.item.linkLabel || row.item.linkAddress }}</b-link>
      </template>
      <template #cell(actions)="row">
        <b-button variant="primary" size="sm" class="me-1" :disabled="isBusy" @click="startEdit(row.item)">Edit</b-button>
        <b-button variant="danger" size="sm" :disabled="isBusy" @click="confirmDelete(row.item)">Delete</b-button>
      </template>
    </b-table>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import type { TableField } from 'bootstrap-vue-next';
import { DateTime } from 'luxon';

import { adminClient, type CreateSiteAnnouncementRequest, type DeleteSiteAnnouncementRequest, type EditSiteAnnouncementRequest, type SiteAnnouncementViewModel } from '@/api/clients';
import { getApiErrorMessage } from '@/utilities/apiError';
import { useConfirm } from '@/utilities/confirm';
import { formatDateTime } from '@/utilities/formatters';

interface AnnouncementForm {
  title: string;
  body: string;
  //The datetime-local input's value, in the browser's zone. Empty when not set.
  postedAt: string;
  linkAddress: string;
  linkLabel: string;
}

interface AdminSiteAnnouncementsData {
  isBusy: boolean;
  errorInfo: string;
  jobAttempted: string;
  announcements: SiteAnnouncementViewModel[] | null;
  editingAnnouncementID: string | null;
  form: AnnouncementForm;
}

function emptyForm(): AnnouncementForm {
  return {
    title: '',
    body: '',
    postedAt: '',
    linkAddress: '',
    linkLabel: ''
  };
}

export default defineComponent({
  setup() {
    const announcementFields: TableField<SiteAnnouncementViewModel>[] = [
      { key: 'title', label: 'Title', thClass: 'bg-primary' },
      { key: 'postedAt', label: 'Posted', thClass: 'bg-primary' },
      { key: 'link', label: 'Link', thClass: 'bg-primary' },
      { key: 'actions', label: '', thClass: 'bg-primary' }
    ];

    return { confirm: useConfirm(), announcementFields };
  },
  data(): AdminSiteAnnouncementsData {
    return {
      isBusy: false,
      errorInfo: '',
      jobAttempted: '',
      announcements: null,
      editingAnnouncementID: null,
      form: emptyForm()
    };
  },
  computed: {
    isEditing(): boolean {
      return !!this.editingAnnouncementID;
    },
    //A new announcement without a time is posted now. An edit keeps its time unless it's changed, so it needs one.
    formIsValid(): boolean {
      return !!this.form.title.trim() && !!this.form.body.trim() && (!this.isEditing || !!this.form.postedAt);
    }
  },
  async created() {
    //Load quietly: the initial load isn't something the admin asked for, so it gets a banner only if it fails.
    try {
      await this.fetchAnnouncements();
    } catch (error) {
      this.jobAttempted = 'Get site announcements';
      this.errorInfo = getApiErrorMessage(error);
    }
  },
  methods: {
    formatDateTime,
    clearForm(): void {
      this.editingAnnouncementID = null;
      this.form = emptyForm();
    },
    startEdit(announcement: SiteAnnouncementViewModel): void {
      this.editingAnnouncementID = announcement.id;
      this.form = {
        title: announcement.title,
        body: announcement.body,
        postedAt: DateTime.fromISO(announcement.postedAt).toFormat("yyyy-MM-dd'T'HH:mm"),
        linkAddress: announcement.linkAddress ?? '',
        linkLabel: announcement.linkLabel ?? ''
      };
      window.scrollTo({ top: 0, behavior: 'smooth' });
    },
    async fetchAnnouncements(): Promise<void> {
      this.announcements = await adminClient.getSiteAnnouncements();
    },
    //Runs one request, with its result in the banner at the top of the page.
    async runAction(label: string, call: () => Promise<void>): Promise<void> {
      this.errorInfo = '';
      this.jobAttempted = label;
      this.isBusy = true;

      try {
        await call();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error);
      } finally {
        this.isBusy = false;
      }
    },
    async refreshAnnouncements(): Promise<void> {
      await this.runAction('Get site announcements', () => this.fetchAnnouncements());
    },
    async createAnnouncement(): Promise<void> {
      if (!this.formIsValid) {
        return;
      }

      const request: CreateSiteAnnouncementRequest = {
        title: this.form.title,
        body: this.form.body,
        postedAt: this.form.postedAt ? new Date(this.form.postedAt).toISOString() : undefined,
        linkAddress: this.form.linkAddress,
        linkLabel: this.form.linkLabel
      };

      await this.runAction('Create site announcement', async () => {
        await adminClient.createSiteAnnouncement(request);
        this.clearForm();
        await this.fetchAnnouncements();
      });
    },
    async saveEdit(): Promise<void> {
      if (!this.formIsValid || !this.editingAnnouncementID) {
        return;
      }

      const request: EditSiteAnnouncementRequest = {
        announcementID: this.editingAnnouncementID,
        title: this.form.title,
        body: this.form.body,
        postedAt: new Date(this.form.postedAt).toISOString(),
        linkAddress: this.form.linkAddress,
        linkLabel: this.form.linkLabel
      };

      await this.runAction('Edit site announcement', async () => {
        await adminClient.editSiteAnnouncement(request);
        this.clearForm();
        await this.fetchAnnouncements();
      });
    },
    async confirmDelete(announcement: SiteAnnouncementViewModel): Promise<void> {
      const confirmed = await this.confirm(`Are you sure you want to delete '${announcement.title}'? It will no longer show on the site or in the RSS feed.`, {
        title: 'Delete Announcement',
        okTitle: 'Delete',
        okVariant: 'danger',
        cancelTitle: 'Cancel'
      });
      if (!confirmed) {
        return;
      }

      const request: DeleteSiteAnnouncementRequest = {
        announcementID: announcement.id
      };

      await this.runAction('Delete site announcement', async () => {
        await adminClient.deleteSiteAnnouncement(request);
        if (this.editingAnnouncementID === announcement.id) {
          this.clearForm();
        }
        await this.fetchAnnouncements();
      });
    }
  }
});
</script>
