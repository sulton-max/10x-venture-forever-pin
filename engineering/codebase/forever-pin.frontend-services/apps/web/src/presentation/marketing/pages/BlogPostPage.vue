<script setup lang="ts">
import { ArrowLeft } from '@lucide/vue';
import { Button } from '@wow-two-beta/ui-vue/presentation/actions';
import { Badge, Heading, Text } from '@wow-two-beta/ui-vue/presentation/display';
import { SurfaceLayout } from '@wow-two-beta/ui-vue/presentation/layout';
import { computed, defineAsyncComponent } from 'vue';
import { RouterLink } from 'vue-router';

import { AppPaths } from '@/presentation/common';
import NotFoundPage from '@/presentation/common/pages/NotFoundPage.vue';

import BlogPostCard from '../blog/BlogPostCard.vue';
import { BlogPosts } from '../blog/BlogPosts';

/** Defines the props of {@link BlogPostPage}. */
export interface BlogPostPageProps {
  /** The article's slug from the address. */
  readonly slug: string;
}

/** Holds the article date format. */
const DateFormat = new Intl.DateTimeFormat('en-US', { year: 'numeric', month: 'long', day: 'numeric' });

/** Renders one article with its call to action and follow-up reading; an unknown slug is not found. */
defineOptions({ name: 'BlogPostPage' });
const props = defineProps<BlogPostPageProps>();

const post = computed(() => BlogPosts.find(props.slug));
const body = computed(() => (post.value ? defineAsyncComponent(post.value.load) : undefined));
const related = computed(() => BlogPosts.related(props.slug, 2));
const published = computed(() => (post.value ? DateFormat.format(new Date(`${post.value.date}T00:00:00`)) : ''));
</script>

<template>
  <NotFoundPage v-if="!post" />
  <article v-else class="mx-auto max-w-2xl px-6 py-14">
    <RouterLink
      :to="AppPaths.blog()"
      class="inline-flex items-center gap-1 text-sm text-muted-foreground transition-colors hover:text-foreground"
    >
      <ArrowLeft :size="15" aria-hidden="true" /> All posts
    </RouterLink>

    <div class="mt-6 flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
      <Badge variant="brand" size="md" class="px-2.5 py-1 text-primary">{{ post.tag }}</Badge>
      <time :datetime="post.date">{{ published }}</time>
      <span aria-hidden="true">·</span>
      <span>{{ post.readingMinutes }} min read</span>
    </div>

    <Heading :level="1" size="2xl" weight="bold" class="mt-4 leading-tight sm:text-4xl">{{ post.title }}</Heading>
    <Text size="lg" color="muted" class="mt-4">{{ post.description }}</Text>

    <hr class="my-8 border-border" />

    <div class="prose">
      <component :is="body" />
    </div>

    <SurfaceLayout variant="soft" tone="primary" class="mt-12 rounded-2xl bg-primary-soft/50 p-6 text-center">
      <Text weight="semibold">Try it on a code that never expires.</Text>
      <Text size="sm" color="muted" class="mt-1">
        Create a programmable QR code in under a minute — free, no account needed.
      </Text>
      <div class="mt-4 flex flex-wrap justify-center gap-3">
        <Button as-child tone="primary" size="sm">
          <RouterLink :to="AppPaths.newCode()">Create a code</RouterLink>
        </Button>
        <Button as-child tone="neutral" variant="outline" size="sm">
          <RouterLink :to="AppPaths.pricing()">See pricing</RouterLink>
        </Button>
      </div>
    </SurfaceLayout>

    <div v-if="related.length > 0" class="mt-12">
      <Heading :level="2" size="md" class="tracking-normal">Keep reading</Heading>
      <div class="mt-4 grid grid-cols-1 gap-6 sm:grid-cols-2">
        <BlogPostCard v-for="entry in related" :key="entry.slug" :post="entry" />
      </div>
    </div>
  </article>
</template>
