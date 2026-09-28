import { RuleConditionType } from '@/domain/codes';

/** Maps each routing condition to its label and value placeholder. */
export const RuleConditionTypeDisplays: Readonly<
  Record<RuleConditionType, { readonly label: string; readonly placeholder: string }>
> = {
  [RuleConditionType.Device]: { label: 'Device', placeholder: 'Ios · Android · Desktop' },
  [RuleConditionType.Country]: { label: 'Country', placeholder: 'US' },
  [RuleConditionType.Language]: { label: 'Language', placeholder: 'ru' },
  [RuleConditionType.TimeOfDay]: { label: 'Time of day', placeholder: '09:00-16:00' },
};
