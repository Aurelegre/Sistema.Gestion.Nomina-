function toggleParent(parentCheckbox, parentId) {
    // Get all child checkboxes associated with this parent
    var childCheckboxes = document.querySelectorAll('input[data-parent-id="' + parentId + '"]');

    childCheckboxes.forEach(function (childCheckbox) {
        childCheckbox.checked = parentCheckbox.checked;
    });
}

function checkParentStatus(parentId) {
    // Get all child checkboxes for this parent
    var childCheckboxes = document.querySelectorAll('input[data-parent-id="' + parentId + '"]');

    // Check if all children are checked
    var allChecked = Array.from(childCheckboxes).every(function (checkbox) {
        return checkbox.checked;
    });

    // Set the parent's checkbox based on children's status
    var parentCheckbox = document.getElementById('parent_' + parentId);
    parentCheckbox.checked = allChecked;
}
