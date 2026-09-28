<script setup lang="ts">
import BlogPostCard from '../blog/BlogPostCard.vue';
import { BlogPosts } from '../blog/BlogPosts';
import MarketingHeading from '../MarketingHeading.vue';
import MarketingSection from '../MarketingSection.vue';

/** Renders the article index, newest first. */
defineOptions({ name: 'BlogIndexPage' });
</script>

<template>
  <MarketingSection>
    <MarketingHeading
      eyebrow="Blog"
      title="Why and how to use QR codes"
      description="Practical guides to getting QR codes right — from the squares that have to scan to the routing rules behind them."
      :level="1"
    />
    <div class="mx-auto mt-12 grid max-w-4xl grid-cols-1 gap-6 sm:grid-cols-2">
      <BlogPostCard v-for="post in BlogPosts.newestFirst" :key="post.slug" :post="post" />
    </div>
  </MarketingSection>
</template>
