<script setup lang="ts">
import { ArrowRight } from '@lucide/vue';
import { Badge, Heading, Text } from '@wow-two-beta/ui-vue/presentation/display';
import { RouterLink } from 'vue-router';

import { AppPaths } from '@/presentation/common';

import type { PostEntry } from './BlogPosts';

/** Defines the props of {@link BlogPostCard}. */
export interface BlogPostCardProps {
  /** The article to link. */
  readonly post: PostEntry;
}

/** Renders an article's listing card; the whole card links to it. */
defineOptions({ name: 'BlogPostCard' });
defineProps<BlogPostCardProps>();
</script>

<template>
  <RouterLink
    :to="AppPaths.blogPost(post.slug)"
    class="group flex flex-col rounded-2xl border border-border bg-card p-6 transition-colors hover:border-primary/40 focus-visible:outline-2 focus-visible:outline-ring"
  >
    <div class="flex items-center gap-2 text-xs text-muted-foreground">
      <Badge variant="brand" size="md" class="px-2.5 py-1 text-primary">{{ post.tag }}</Badge>
      <span>{{ post.readingMinutes }} min read</span>
    </div>
    <Heading :level="3" size="md" class="mt-4 leading-snug tracking-normal group-hover:text-primary">
      {{ post.title }}
    </Heading>
    <Text size="sm" color="muted" class="mt-2 flex-1 leading-relaxed">{{ post.description }}</Text>
    <span class="mt-4 inline-flex items-center gap-1 text-sm font-medium text-primary">
      Read <ArrowRight :size="15" class="transition-transform group-hover:translate-x-0.5" aria-hidden="true" />
    </span>
  </RouterLink>
</template>
