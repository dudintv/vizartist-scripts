## Link window mask targets

EXAMPLE:

```
root
-- child1
---- el_with_win_mask    (copy here)
-- child2
---- el_with_win_mask    (paste here)
-- child3
---- el_with_win_mask    (paste here)
```

Use cases:

1) in Viz 3.14, the WindowMask looses its connection to the target when you use Coco plugin to duplicate containers.
2) in other versions, if you need to update targets correspondigly the relative locations withinin the local sub-tree. For example, to imitate "NEXT" or "PREV" relations.

#### Version 1.0.1 (12 September 2025)
* ignore Window Masks without targets

#### Version 1.0.0 (1 September 2025)
* can copy-paste Window Mask targets from the first subcontainer childs to childes of the other subcontainers the rest items
