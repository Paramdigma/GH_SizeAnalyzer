---
layout: page
---

<script setup>
import {
  VPTeamPage,
  VPTeamPageTitle,
  VPTeamMembers
} from 'vitepress/theme'

const members = [
  {
    avatar: 'https://www.github.com/christiandimitri.png',
    name: 'Christian Dimitri',
    title: 'Creator',
    links: [
      { icon: 'github', link: 'https://github.com/christiandimitri' },
    ]
  },
  {
    avatar: 'https://www.github.com/alanrynne.png',
    name: 'Alan Rynne',
    title: 'AEC Software Developer',
    links: [
      { icon: 'github', link: 'https://github.com/alanrynne' },
      { icon: 'twitter', link: 'https://twitter.com/alanrynne' }
    ]
  }
]
</script>

<VPTeamPage>
  <VPTeamPageTitle>
    <template #title>
      Our Team
    </template>
    <template #lead>
      The development of GH_SizeAnalyzer is driven by the <a href="https://github.com/paramdigma">Paramdigma</a> team
    </template>
  </VPTeamPageTitle>
  <VPTeamMembers
    :members="members"
  />
</VPTeamPage>
