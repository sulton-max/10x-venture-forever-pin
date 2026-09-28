import { onScopeDispose, shallowRef, toValue, watch, type MaybeRefOrGetter, type Ref } from 'vue';

/** Mirrors `source` once it has stopped changing for `ms`; typing a search runs one request per pause. */
export function useSettledValue<T>(source: MaybeRefOrGetter<T>, ms: number): Readonly<Ref<T>> {
  const settled = shallowRef(toValue(source));
  let timer: ReturnType<typeof setTimeout> | undefined;
  watch(
    () => toValue(source),
    (value) => {
      clearTimeout(timer);
      timer = setTimeout(() => (settled.value = value), ms);
    },
  );
  onScopeDispose(() => clearTimeout(timer));
  return settled;
}
